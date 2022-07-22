using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;
using Sync;
using System.Text;

public class NetPacket : MonoBehaviour
{
    [SerializeField]private TCPConn conn;

    private readonly object packetLock = new object();
    private byte[] readBuffer = new byte[4096];
    private int ready = 0;
    private List<Tuple<ushort, byte[]>> packets = new List<Tuple<ushort, byte[]>>();

    public delegate void RecvMatchResp(MatchResp match);
    public delegate void RecvMove(MovePiece match);
    public delegate void RecvGameSetupNotify(GameSetupNotify setupNotify);
    public delegate void RecvPromote(Promote promote);
    
    public event RecvMatchResp OnRecvMatchResp;
    public event RecvMove OnRecvMove;
    public event RecvGameSetupNotify OnRecvGameSetupNotify;
    public event RecvPromote OnRecvPromote;

    private void Start()
    {
        OnRecvMatchResp += (MatchResp e) => { Debug.Log("start match result: " + e.Ret); };
    }

    public static ushort GetMessageProto(string msgTypeName)
    {
        MessageID proto;
        switch(msgTypeName)
        {
            case "Match":
                proto = MessageID.Match;
                break;
            case "MovePiece":
                proto = MessageID.Move;
                break;
            case "CancelMatch":
                proto = MessageID.CancelMatch;
                break;
            case "Promote":
                proto = MessageID.Promote;
                break;
            default:
                proto = MessageID.Padding;
                break;
        }

        return (ushort)proto;
    }

    public byte[] HeaderPadding(ushort proto, byte[] msg)
    {
        byte[] data = new byte[msg.Length + 4];
        var sizeBytes = BitConverter.GetBytes((ushort)msg.Length);
        var protoBytes = BitConverter.GetBytes(proto);

        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(sizeBytes);
            Array.Reverse(protoBytes);
        }

        Buffer.BlockCopy(sizeBytes, 0, data, 0, sizeBytes.Length);
        Buffer.BlockCopy(protoBytes, 0, data, sizeBytes.Length, protoBytes.Length);
        Buffer.BlockCopy(msg, 0, data, sizeBytes.Length + protoBytes.Length, msg.Length);

        return data;
    }

    public bool PeekMessage()
    {
        while(ready >= 4)
        {
            var sizeBytes = new byte[2];
            Buffer.BlockCopy(readBuffer, 0, sizeBytes, 0, 2);
            Array.Reverse(sizeBytes);
            ushort size = BitConverter.ToUInt16(sizeBytes, 0);

            var protoIDBytes = new byte[2];
            Buffer.BlockCopy(readBuffer, 2, protoIDBytes, 0, 2);
            Array.Reverse(protoIDBytes);
            ushort protoID = BitConverter.ToUInt16(protoIDBytes, 0);

            Debug.LogFormat("parse message size: {0}, proto id: {1}", size, protoID);

            if (ready < 4 + size)
                return false;

            var msgBytes = new byte[size];
            Buffer.BlockCopy(readBuffer, 4, msgBytes, 0, size);

            lock(packetLock)
            {
                packets.Add(new Tuple<ushort, byte[]>(protoID, msgBytes));
            }

            Buffer.BlockCopy(readBuffer, 4 + size, readBuffer, 0, ready - 4 - size);
            ready = ready - 4 - size;

            return true;
        }

        return false;   
    }

    public void DispatchMessage()
    {
        lock(packetLock)
        {
            foreach (var packet in packets)
            {
                try
                {
                    //Debug.Log("bin data: " + BitConverter.ToString(packet.Item2));
                    switch (packet.Item1)
                    {
                        case 1:
                            var matchResp = new MatchResp();
                            matchResp.MergeFrom(packet.Item2);
                            Debug.Log("recv match resp: " + matchResp);
                            OnRecvMatchResp?.Invoke(matchResp);
                            break;
                        case 2:
                            var moveSync = new MovePiece();

                            moveSync.MergeFrom(packet.Item2);
                            Debug.Log("recv move resp: " + moveSync);
                            OnRecvMove?.Invoke(moveSync);
                            break;
                        case 3:
                            var cancelResp = new CancelMatchResp();
                            cancelResp.MergeFrom(packet.Item2);
                            Debug.Log("recv cancel match resp: " + cancelResp);
                            break;
                        case 4:
                            var gameSetupNotify = new GameSetupNotify();
                            gameSetupNotify.MergeFrom(packet.Item2);
                            Debug.Log("recv game setup notify");
                            OnRecvGameSetupNotify?.Invoke(gameSetupNotify);
                            break;
                        case 5:
                            var promoteSync = new Promote();
                            promoteSync.MergeFrom(packet.Item2);
                            Debug.Log("recv promote from opponent");
                            OnRecvPromote?.Invoke(promoteSync);
                            break;
                        default:
                            Debug.Log("invalid proto id: " + packet.Item1);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            packets.Clear();
        }
    }

    public void Input(byte[] source, int len)
    {
        Buffer.BlockCopy(source, 0, readBuffer, ready, len);
        ready += len;
        PeekMessage();
    }

    private void FixedUpdate()
    {
        DispatchMessage();
    }

    public void StartMatch(int side)
    {
        var match = new Match();
        if (side == 2)
            match.Random = true;
        else
            match.PickColor = side;

        conn.SendData(HeaderPadding(GetMessageProto(match.GetType().Name), match.ToByteArray()));
    }

    public void CancelMatch()
    {
        var cancel = new CancelMatch();

        conn.SendData(HeaderPadding(GetMessageProto(cancel.GetType().Name), cancel.ToByteArray()));
    }

    public void SyncMove(Tuple<int, int> origin, Tuple<int, int> movement, UnityEngine.Color color)
    {
        var move = new MovePiece();
        move.OriginX = origin.Item1;
        move.OriginY = origin.Item2;
        move.X = movement.Item1;
        move.Y = movement.Item2;
        move.Color = color == UnityEngine.Color.white ? Sync.Color.White : Sync.Color.Black;

        conn.SendData(HeaderPadding(GetMessageProto(move.GetType().Name), move.ToByteArray()));
    }

    public void SyncPromoteAct(string tag, Tuple<int, int> promotePos)
    {
        var promote = new Promote();    
        promote.Tag = tag;  
        promote.X = promotePos.Item1;
        promote.Y = promotePos.Item2;

        //Debug.Log("send promote info: " + promote.ToString());

        conn.SendData(HeaderPadding(GetMessageProto(promote.GetType().Name), promote.ToByteArray()));
    }
}
