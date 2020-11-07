﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Valve.Sockets;
using AOT;
using System.Linq;

public enum DisconnectionReason { NONE, ERROR, SERVER_CLOSED, SERVER_FULL, KICKED };

public class ServerController : MonoBehaviour
{
    public string version = "0.1";
    public ushort port = 10069;

    private StatusCallback status;
    private NetworkingSockets server;
    NetworkingUtils utils;
    private uint listenSocket;

    //private uint connectedPollGroup;

    public List<UInt32> connectedClients = new List<UInt32>();

    public bool acceptingClients = true;
    public int maxConnections = 100;
    public int maxPlayers = 20;

    public static ServerController Instance;

    const int maxMessages = 40;
    Valve.Sockets.NetworkingMessage[] netMessages = new Valve.Sockets.NetworkingMessage[maxMessages];

    DebugCallback debug = (type, message) => {
        Debug.Log("Networking Debug - Type: " + type + ", Message: " + message);
    };


    void Awake()
    {
        Library.Initialize();

        Debug.Log("Initialized ValveSockets");
    }

    void Start()
    {
        Instance = this;
        StartServer();
    }

    void StartServer()
    {
        Debug.Log("Starting server...");

        utils = new NetworkingUtils();
        utils.SetDebugCallback(DebugType.Message, debug);

        status = OnServerStatusUpdate;
        utils.SetStatusCallback(status);

        server = new NetworkingSockets();

        Address address = new Address();
        address.SetAddress("::0", port);

        listenSocket = server.CreateListenSocket(ref address);

        //connectedPollGroup = server.CreatePollGroup();
    }

    [MonoPInvokeCallback(typeof(StatusCallback))]
    static void OnServerStatusUpdate(ref StatusInfo info)
    {
        // Debug.Log("Server Status: " + info.ToString());
        switch(info.connectionInfo.state)
        {
            case Valve.Sockets.ConnectionState.None:
                break;

            case Valve.Sockets.ConnectionState.Connecting:

                if(Instance.acceptingClients && Instance.connectedClients.Count < Instance.maxConnections)
                {
                    Result r = Instance.server.AcceptConnection(info.connection);
                    Debug.Log("Server Accept connection result " + r.ToString());
                    //server.SetConnectionPollGroup(connectedPollGroup, info.connection);
                    Instance.connectedClients.Add(info.connection);
                } else
                {
                    Instance.server.CloseConnection(info.connection, (int)DisconnectionReason.SERVER_FULL, "Server full.", false);
                }

                break;

            case Valve.Sockets.ConnectionState.Connected:
                Debug.Log(String.Format("Client connected - ID: {0}, IP: {1}", info.connection, info.connectionInfo.address.GetIP()));
                break;

            case Valve.Sockets.ConnectionState.ClosedByPeer:
            case Valve.Sockets.ConnectionState.ProblemDetectedLocally:

                string closeDebug = "";
                DisconnectionReason reason = 0;

                if(info.connectionInfo.state == Valve.Sockets.ConnectionState.ProblemDetectedLocally)
                {
                    closeDebug = "Problem detected locally.";
                    reason = DisconnectionReason.ERROR;
                } else
                {
                    closeDebug = "Closed by peer.";
                    reason = DisconnectionReason.NONE;
                }

                Instance.RemoveClient(info.connection, reason, closeDebug);
                Debug.Log(String.Format("Client disconnected from server - ID: {0}, IP: {1}", info.connection, info.connectionInfo.address.GetIP()));
                break;
        }
    }

    void OnMessage(ref Valve.Sockets.NetworkingMessage netMessage)
    {
        // Debug.Log(String.Format("Message received server - ID: {0}, Channel ID: {1}, Data length: {2}", netMessage.connection, netMessage.channel, netMessage.length));

        byte[] messageDataBuffer = new byte[netMessage.length];

        netMessage.CopyTo(messageDataBuffer);
        netMessage.Destroy();

        try
        {
            NetworkingMessage msg = NetworkingMessageTranslator.ParseMessage(messageDataBuffer);

            UInt32 clientID = netMessage.connection;

            switch(msg.type)
            {
                case NetworkingMessageType.CLIENT_JOIN:


                    break;

                case NetworkingMessageType.INPUT_STATE:


                    break;

                case NetworkingMessageType.GLOBAL_LEADERBOARD:

                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }

        // Debug.Log(result);
    }

    public void SendTo(UInt32 connectionID, string data, SendFlags flags)
    {
        SendTo(connectionID, Encoding.ASCII.GetBytes(data), flags);
    }

    public void SendTo(UInt32 connectionID, byte[] data, SendFlags flags)
    {
        server.SendMessageToConnection(connectionID, data, flags);
    }

    public void SendToAll(Byte[] data, SendFlags flags)
    {
        if (ServerActive())
        {
            foreach (UInt32 connectedId in connectedClients.ToArray())
            {
                SendTo(connectedId, data, flags);
            }
        }
    }

    public void SendToAllPlayers(Byte[] data, SendFlags flags)
    {
        if (ServerActive())
        {
            // TODO FIX THIS... not new list duh!
            foreach (UInt32 connectedId in new List<UInt32>())
            {
                SendTo(connectedId, data, flags);
            }
        }
    }

    public bool ServerActive()
    {
        return server != null;
    }

    public void RemoveClient(UInt32 connectionID, DisconnectionReason reason, string debug)
    {
        if (connectedClients.Contains(connectionID)) {
            connectedClients.Remove(connectionID);
            server.CloseConnection(connectionID, (int)reason, debug, false);
        }
    }

    void Update()
    {
        Receive();
    }

    void Receive()
    {
        if (ServerActive())
        {
            server.RunCallbacks();

            for(int c = 0; c < connectedClients.Count; ++c){
                int netMessagesCount = server.ReceiveMessagesOnConnection(connectedClients[c], netMessages, maxMessages);

                if (netMessagesCount > 0)
                {
                    for (int i = 0; i < netMessagesCount; ++i)
                    {
                        OnMessage(ref netMessages[i]);
                    }
                }
            }

            {
                int netMessagesCount = server.ReceiveMessagesOnConnection(listenSocket, netMessages, maxMessages);

                if (netMessagesCount > 0)
                {
                    for (int i = 0; i < netMessagesCount; ++i)
                    {
                        OnMessage(ref netMessages[i]);
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        server.CloseListenSocket(listenSocket);
        Library.Deinitialize();
    }
}
