using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using Valve.Sockets;
using AOT;

[Serializable]
public enum ClientState { IDLE, CONNECTING, CONNECTED, DISCONNECTING, ERROR };

public delegate void OnConnect(bool connected);
public delegate void OnDisconnect();
public delegate void OnReject(string reason);

public class ClientController : MonoBehaviour
{
    public string serverIP;
    public ushort serverport = 10069;

    private static StatusCallback status;
    static NetworkingSockets client;
    static NetworkingUtils utils;
    public static UInt32 connection;

    public OnConnect onConnect;
    public OnDisconnect onDisconnect;
    public OnReject onReject;

    public static ClientController Instance;

    ClientGameRunner cgr;

    const int maxMessages = 40;
    Valve.Sockets.NetworkingMessage[] netMessages = new Valve.Sockets.NetworkingMessage[maxMessages];

    DebugCallback debug = (type, message) => {
        Debug.Log("Networking Debug - Type: " + type + ", Message: " + message);
    };

    public bool connected = false;

    void Awake()
    {
        Instance = this;

        Library.Initialize();

        Debug.Log("Initialized ValveSockets");

        cgr = GetComponent<ClientGameRunner>();
    }

    void Start()
    {

    }

    public void ConnectToServer(string username, OnConnect onConnect, OnDisconnect onDisconnect, OnReject onReject)
    {
        this.onConnect = onConnect;
        this.onDisconnect = onDisconnect;
        this.onReject = onReject;

        utils = new NetworkingUtils();
        utils.SetDebugCallback(DebugType.Message, debug);

        Reset();

        client = new NetworkingSockets();

        Address address = new Address();

        address.SetAddress(serverIP, serverport);

        status = OnClientStatusUpdate;
        utils.SetStatusCallback(status);

        connection = client.Connect(ref address);
    }

    public void JoinGame(string username, OnConnect onConnect, OnDisconnect onDisconnect, OnReject onReject)
    {
        this.onConnect = (bool connected) => {
            this.onConnect = onConnect;

            if (connected)
            {
                onConnect?.Invoke(connected);
            }
            else
            {
                onConnect?.Invoke(connected);
            }
        };

        ConnectToServer(username, this.onConnect, onDisconnect, onReject);
    }

    [MonoPInvokeCallback(typeof(StatusCallback))]
    static void OnClientStatusUpdate(ref StatusInfo info)
    {
        switch (info.connectionInfo.state)
        {
            case Valve.Sockets.ConnectionState.None:
                break;

            case Valve.Sockets.ConnectionState.Connecting:
                break;

            case Valve.Sockets.ConnectionState.Connected:
                Debug.Log(String.Format("Connected to server - ID: {0}, IP: {1}", connection, info.connectionInfo.address.GetIP()));

                Instance.connected = true;
                Instance.onConnect?.Invoke(Instance.connected);
                break;

            case Valve.Sockets.ConnectionState.ClosedByPeer:

                Instance.onDisconnect?.Invoke();
                Instance.Disconnect();

                Debug.Log(String.Format("Disconnected from server (closed by server) - ID: {0}, IP: {1}", connection, info.connectionInfo.address.GetIP()));
                break;

            case Valve.Sockets.ConnectionState.ProblemDetectedLocally:

                if(!Instance.connected)
                    Instance.onConnect?.Invoke(Instance.connected);
                else
                    Instance.onDisconnect?.Invoke();

                Instance.Disconnect();

                Debug.Log(String.Format("Disconnected from server (error) - ID: {0}, IP: {1}", connection, info.connectionInfo.address.GetIP()));
                break;
        }
    }

    void OnMessage(ref Valve.Sockets.NetworkingMessage netMessage)
    {
        // Debug.Log(String.Format("Message received client - ID: {0}, Channel ID: {1}, Data length: {2}", netMessage.connection, netMessage.channel, netMessage.length));

        byte[] messageDataBuffer = new byte[netMessage.length];

        netMessage.CopyTo(messageDataBuffer);
        netMessage.Destroy();

        try
        {
            NetworkingMessage msg = NetworkingMessageTranslator.ParseMessage(messageDataBuffer);

            switch(msg.type)
            {
                case NetworkingMessageType.GAME_STATE:

                    GameState gs = (GameState)NetworkingMessageTranslator.ByteArrayToObject(msg.content);

                    Instance.cgr.ReceiveGameState(gs, msg.frame);

                    break;

                case NetworkingMessageType.SERVER_SEND_ID:

                    Instance.cgr.playerID = (uint)NetworkingMessageTranslator.ByteArrayToObject(msg.content);

                    Debug.Log("Client received client ID: " + Instance.cgr.playerID);

                    break;

                case NetworkingMessageType.ENTITY_DATA:

                    List<EntityData> ed = (List<EntityData>)NetworkingMessageTranslator.ByteArrayToObject(msg.content);

                    Instance.cgr.ReceiveEntityData(ed, msg.frame);

                    break;

                case NetworkingMessageType.ENTITY_STATE:

                    List<EntityState> es = (List<EntityState>)NetworkingMessageTranslator.ByteArrayToObject(msg.content);

                    Instance.cgr.ReceiveEntityState(es, msg.frame);

                    break;

                case NetworkingMessageType.PLAYER_DATA:

                    List<PlayerData> pd = (List<PlayerData>)NetworkingMessageTranslator.ByteArrayToObject(msg.content);

                    Instance.cgr.ReceivePlayerData(pd, msg.frame);

                    break;

                case NetworkingMessageType.PLAYER_STATE:

                    List<PlayerState> ps = (List<PlayerState>)NetworkingMessageTranslator.ByteArrayToObject(msg.content);

                    Instance.cgr.ReceivePlayerState(ps, msg.frame);

                    break;

            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    void Update()
    {
        Receive();
    }

    void Receive()
    {
        if(client != null)
        {
            client.RunCallbacks();

            int netMessagesCount = client.ReceiveMessagesOnConnection(connection, netMessages, maxMessages);

            if (netMessagesCount > 0)
            {
                for (int i = 0; i < netMessagesCount; ++i)
                {
                    OnMessage(ref netMessages[i]);
                }
            }
        }        
    }

    void Reset()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        if(client != null)
        {
            bool disconnected = client.CloseConnection(connection, (int)DisconnectionReason.NONE, "client disconnected", false);
            Debug.Log("Client side disconnection was " + disconnected);
        }

        connected = false;
    }

    public void Send(Byte[] data, SendFlags flags, OnSent onSent)
    {
        if(connected)
        {
            client.SendMessageToConnection(connection, data, flags);
            onSent?.Invoke();
        }
    }

    void Close()
    {
        Disconnect();
    }

    void OnApplicationQuit()
    {
        Library.Deinitialize();
    }
}