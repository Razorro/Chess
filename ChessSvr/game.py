import random

import data_pb2

import codec


class MatchInfo:
    def __init__(self, address):
        self.id = address


class Match:
    def __init__(self, addresses, sides=None):
        self.id = id(self)
        if sides is None:
            sides = list(range(0, 2))
            random.shuffle(sides)
            
        self.player2seat = {}
        for i in range(len(addresses)):
            self.player2seat[addresses[i]] = sides[i]

        self.seat2player = {seat: player for player, seat in self.player2seat.items()}
        self.ready = set()

    def getOpponent(self, address):
        seat = self.player2seat[address]
        return self.seat2player[(seat ^ 1)]


class GameManager:
    def __init__(self, svr):
        self.svr = svr

        self.matches = {}
        self.random_queue = []
        self.white_queue = []
        self.black_queue = []

        self.id2queue = {}
        self.id2game = {}

    def playerLeave(self, address):
        if address in self.id2game:
            match = self.id2game[address]
        
            opponent = match.getOpponent(address)
            del self.id2game[opponent]
            del self.id2game[address]
            del self.matches[match.id]

        elif address in self.id2queue:
            queue_id = self.id2queue[address]
            del self.id2queue[address]
            if queue_id == 0:
                self.white_queue.remove(address)
            elif queue_id == 1:
                self.black_queue.remove(address)
            else:
                self.random_queue.remove(address)

    def queue2game(self, match, player_id):
        print("queue2game:", player_id)
        del self.id2queue[player_id]
        self.id2game[player_id] = match

    def notifyGameSetup(self, match):
        setupNotify = data_pb2.GameSetupNotify()
        for player_id, side in match.player2seat.items():
            setupNotify.side = side
            self.svr.forward(player_id, codec.message2id[data_pb2.GameSetupNotify], setupNotify)

    def checkMatchStart(self):
        newMatches = []
        randomMatchIdx = 1
        while randomMatchIdx < len(self.random_queue):
            addresses = self.random_queue[randomMatchIdx-1], self.random_queue[randomMatchIdx]
            randomMatchIdx += 2

            match = Match(addresses)
            newMatches.append(match)
            self.matches[match.id] = match
            for player_id in addresses:
                self.queue2game(match, player_id)
        self.random_queue = self.random_queue[randomMatchIdx-1:]

        white_idx, black_idx = 0, 0
        while white_idx < len(self.white_queue) and black_idx < len(self.black_queue):
            white = self.white_queue[white_idx]
            white_idx += 1

            black = self.black_queue[black_idx]
            black_idx += 1

            addresses = (white, black)
            seats = (0, 1)
            match = Match(addresses, seats)
            newMatches.append(match)
            self.matches[match.id] = match
            for player_id in addresses:
                self.queue2game(match, player_id)
        self.white_queue = self.white_queue[white_idx:]
        self.black_queue = self.black_queue[black_idx:]

        return newMatches

    def enqueueMatch(self, who, msg):
        if msg.random:
            self.random_queue.append(who)
            self.id2queue[who] = 2
            # self.id2queue[who] = [len(self.random_queue)-1, 2]
        elif msg.pickColor == 0:
            self.white_queue.append(who)
            self.id2queue[who] = 0
            # self.id2queue[who] = [len(self.white_queue)-1, 0]
        else:
            self.black_queue.append(who)
            self.id2queue[who] = 1
            # self.id2queue[who] = [len(self.black_queue)-1, 1]

        print(f'{who} into queue')          

    def dequeueMatch(self, who, msg):
        if who not in self.id2queue:
            print(f"{who} not in match queue")
            return

        queue_id = self.id2queue[who]
        if queue_id == 0:
            self.white_queue.pop(self.white_queue.index(who)) 
        elif queue_id == 1:
            self.black_queue.pop(self.black_queue.index(who)) 
        elif queue_id == 2:
            self.random_queue.pop(self.random_queue.index(who))

        del self.id2queue[who]

        print(f"{who} leave queue")

    def startMatch(self, who, msg):
        resp = data_pb2.MatchResp()
        if who in self.id2queue:
            print("already in match queue")
            return resp

        if who in self.id2game:
            print("already in game")
            return resp

        self.enqueueMatch(who, msg)
        newMatches = self.checkMatchStart()
        print("new match:", newMatches)
        if newMatches:
            for match in newMatches:
                self.notifyGameSetup(match)
        
        resp.ret = True
        return resp

    def relayMsg(self, who, msg):
        if who not in self.id2game:
            print("not in a game")
            return

        match = self.id2game[who]
        opponent = match.getOpponent(who)
        self.svr.forward(opponent, codec.message2id[msg.__class__], msg)

    def cancelMatch(self, who, msg):
        resp = data_pb2.CancelMatchResp()
        if who not in self.id2queue:
            print("not in a match queue, cannot cancel match")
            return resp

        self.dequeueMatch(who, msg)
        resp.ret = True

        return resp

    def dealMessage(self, who, proto_id, msg):
        if proto_id == 1:
            return self.startMatch(who, msg)
        elif proto_id == 2:
            return self.relayMsg(who, msg)
        elif proto_id == 3:
            return self.cancelMatch(who, msg)
        elif proto_id == 5:
            return self.relayMsg(who, msg)
    