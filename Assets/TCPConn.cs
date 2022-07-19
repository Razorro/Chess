using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

public class TCPConn : MonoBehaviour
{
    public string host;
    public int port;
    private Socket _socket;
    private IPEndPoint _ip;

    [SerializeField] private NetPacket codec;
    private byte[] recvBuffer = new byte[4096];
    private byte[] sendBuffer = new byte[4096];
    int written = 0;
    
    private Thread readThread;

    // Start is called before the first frame update
    void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _ip = new IPEndPoint(IPAddress.Parse(host), port);

        try
        {
            _socket.Connect(_ip);
        }
        catch(Exception)
        {
            Debug.LogError("Connect to host fail, current status: " + _socket.Connected);
        }

        readThread = new Thread(new ThreadStart(NetworkRead));
        readThread.Start();
    }

    void NetworkRead()
    {
        while(true)
        {
            if(!_socket.Connected)
            {
                Debug.Log("connection invalid, try to reconnect");
                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(_ip);
                }
                catch (Exception)
                {
                    _socket.Close();
                    Thread.Sleep(5000);
                    continue;
                }
            }

            try
            {
                int read = _socket.Receive(recvBuffer, recvBuffer.Length, SocketFlags.None);
                if (read > 0)
                {
                    Debug.Log("recv data bytes: " + read);
                    codec.Input(recvBuffer, read);
                }

                else
                {
                    _socket.Close();
                    Debug.Log("disconnected from server, connect status: " + _socket.Connected);
                    continue;
                }
            }
            catch (ThreadAbortException)
            {
                Debug.Log("stop reading thread");
            }
            catch(Exception e)
            {
                Debug.Log("socket connect status: " + _socket.Connected);
                Debug.LogError("Network read error: " + e);
                continue;
            }
        }
    }

    void FixedUpdate()
    {
        if(_socket.Connected)
        {
            if (written > 0)
            {
                int sent = _socket.Send(sendBuffer, written, SocketFlags.None);
                written -= sent;
                //Debug.LogFormat("{0} bytes sent, current written index {1}", sent, written);
                Buffer.BlockCopy(sendBuffer, 0, sendBuffer, sent, written);
            }
        }
    }

    private void OnApplicationQuit()
    {
        readThread.Abort();

        if(_socket.Connected)
            _socket.Close();
    }

    public void SendData(byte[] msg)
    {
        if (written >= sendBuffer.Length)
            Debug.LogError("write buffer is full");
        else
        {
            Buffer.BlockCopy(msg, 0, sendBuffer, written, msg.Length);
            written += msg.Length;
        }
    }
}
