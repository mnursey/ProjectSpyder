using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public delegate void OnSent();

public static class NetworkingMessageTranslator
{
    static BinaryFormatter bf = new BinaryFormatter();

    public static byte[] ToByteArray(object obj)
    {
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static System.Object ByteArrayToObject(byte[] arrBytes)
    {
        using (var memStream = new MemoryStream())
        {
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = bf.Deserialize(memStream);
            return obj;
        }
    }

    private static string ToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static byte[] GenerateGameStateNetworkingMessage(GameState gs, int frame)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.GAME_STATE;
        nm.content = ToByteArray(gs);
        nm.frame = frame;

        return ToByteArray(nm);
    }

    public static byte[] GenerateEntityStateNetworkingMessage(List<EntityState> es, int frame)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.ENTITY_STATE;
        nm.content = ToByteArray(es);
        nm.frame = frame;

        return ToByteArray(nm);
    }

    public static byte[] GenerateEntityDataNetworkingMessage(List<EntityData> ed, int frame)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.ENTITY_DATA;
        nm.content = ToByteArray(ed);
        nm.frame = frame;

        return ToByteArray(nm);
    }

    public static byte[] GeneratePlayerStateNetworkingMessage(List<PlayerState> ps, int frame)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.PLAYER_STATE;
        nm.content = ToByteArray(ps);
        nm.frame = frame;

        return ToByteArray(nm);
    }

    public static byte[] GeneratePlayerDataNetworkingMessage(List<PlayerData> pd, int frame)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.PLAYER_DATA;
        nm.content = ToByteArray(pd);
        nm.frame = frame;

        return ToByteArray(nm);
    }

    public static byte[] GenerateClientIDNetworkingMessage(uint id)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.SERVER_SEND_ID;
        nm.content = ToByteArray(id);

        return ToByteArray(nm);
    }

    public static byte[] GenerateUsernameNetworkingMessage(string username)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.CLIENT_USERNAME;
        nm.content = ToByteArray(username);

        return ToByteArray(nm);
    }

    public static NetworkingMessage ParseMessage(byte[] data)
    {
        return (NetworkingMessage)ByteArrayToObject(data);
    }
}

[Serializable]
public enum NetworkingMessageType { ENTITY_STATE, PLAYER_STATE, GAME_STATE, ENTITY_DATA, PLAYER_DATA, USER_COMMANDS, CLIENT_USERNAME, SERVER_SEND_ID };

[Serializable]
public class NetworkingMessage
{
    public NetworkingMessageType type;
    public byte[] content;
    public UInt32 clientID;
    public int frame;

    public NetworkingMessage()
    {

    }

    public NetworkingMessage(NetworkingMessageType type, UInt32 clientID)
    {
        this.type = type;
        this.clientID = clientID;
    }

    public NetworkingMessage(NetworkingMessageType type, UInt32 clientID, byte[] content)
    {
        this.type = type;
        this.clientID = clientID;
        this.content = content;
    }
}


[Serializable]
public class SVector3
{
    public float x;
    public float y;
    public float z;

    public SVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }

    public Vector3 GetValue()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class GameState
{
    public GameStateEnum state;
    public float zoneSize;

    public GameState () {

    }

    public GameState(GameStateEnum state)
    {
        this.state = state;
    }

    public GameState(GameStateEnum state, float zoneSize)
    {
        this.state = state;
        this.zoneSize = zoneSize;
    }
}