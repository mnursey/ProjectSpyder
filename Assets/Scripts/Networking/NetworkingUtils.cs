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

    public static byte[] GenerateGameStateNetworkingMessage(GameState gs)
    {
        NetworkingMessage nm = new NetworkingMessage();
        nm.type = NetworkingMessageType.GAME_STATE;
        nm.content = ToByteArray(gs);

        return ToByteArray(nm);
    }

    public static NetworkingMessage ParseMessage(byte[] data)
    {
        return (NetworkingMessage)ByteArrayToObject(data);
    }
}

[Serializable]
public enum NetworkingMessageType { ENTITY_STATE, PLAYER_STATE, GAME_STATE, ENTITY_DATA, PLAYER_DATA, USER_COMMANDS };

[Serializable]
public class NetworkingMessage
{
    public NetworkingMessageType type;
    public byte[] content;
    public UInt32 clientID;

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
    public string data;

    public GameState () {

    }

    public GameState(string state)
    {
        data = state;
    }
}