import struct

from gevent import monkey
from gevent.server import StreamServer
monkey.patch_all()

import codec
import game


class Player:
    HeaderFormat = '>HH'
    HeaderSize = struct.calcsize(HeaderFormat)

    def __init__(self, svr, sock, address):
        self.svr = svr
        self.sock = sock
        self.address = address
        self.buffer = b''

        self.gameRef = self.svr.gameManager
    
    def peekMsg(self):
        if len(self.buffer) >= self.HeaderSize:
            packet_size, proto_id = struct.unpack(self.HeaderFormat, self.buffer[:self.HeaderSize])
            if len(self.buffer) - self.HeaderSize >= packet_size:
                raw_data, self.buffer = self.buffer[self.HeaderSize:self.HeaderSize+packet_size], self.buffer[self.HeaderSize+packet_size:]
                return (proto_id, raw_data)
            else:
                return False
        else:
            return False

    def sendMsg(self, proto_id, msg):
        binMsg = msg.SerializeToString()
        sent = struct.pack(self.HeaderFormat, len(binMsg), proto_id)
        sent += binMsg
        sent_bytes = 0
        while sent_bytes < len(sent):
            out = self.sock.send(sent)
            sent_bytes += out
            sent = sent[out:]
        print(f"send to {self.sock.getpeername()} data:", msg)

    def processMsg(self):
        while True:
            # try:
                msg = self.sock.recv(8192)
                if not msg:
                    break
                self.buffer += msg
                
                ret = self.peekMsg()
                if ret:
                    proto_id, msg = ret[0], codec.parseMsg(ret[0], ret[1])
                    print("receive message:", proto_id)
                    resp = self.gameRef.dealMessage(self.address, proto_id, msg)
                    if resp:
                        self.sendMsg(proto_id, resp)
            # except Exception as e:
            #     print_exec()
            #     print("logic error:", e)
            #     continue

        print("disconnected from", self.address)


class GameServer:
    def __init__(self, host, port):
        self.svr = StreamServer((host, port), self.handle)

        self.conn = {}
        self.gameManager = game.GameManager(self)
    
    def forward(self, address, proto_id, msg):
        if address not in self.conn:
            print(f"not find {address} in conn")
            return

        self.conn[address].sendMsg(proto_id, msg)

    def run(self):
        self.svr.serve_forever()

    def handle(self, sock, address):
        player = Player(self, sock, address)
        self.conn[address] = player
        print("connection establish:", address)

        player.processMsg()

        self.gameManager.playerLeave(address)
        del self.conn[address]

        return

if __name__ == '__main__':
    svr = GameServer('192.168.10.199', 20000)
    svr.run()

