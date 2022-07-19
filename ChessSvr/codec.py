import data_pb2

id2message = {
    1: data_pb2.Match,
    2: data_pb2.MovePiece,
    3: data_pb2.CancelMatch,
    4: data_pb2.GameSetupNotify,
    5: data_pb2.Promote,
}

message2id = { msgCls: proto_id for proto_id, msgCls in id2message.items() }

def parseMsg(proto_id, raw_data):
    if proto_id not in id2message:
        print("receive invalid proto id:", proto_id)
        return None
    
    msg = id2message[proto_id]()
    msg.ParseFromString(raw_data)
    return msg


if __name__ == '__main__':
    print(id2message)
    test = id2message[1]()
    test.random = True
    print(test)