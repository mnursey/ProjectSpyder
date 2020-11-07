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

    public static byte[] GeneratePingMessage(UInt32 clientID)
    {
        NetworkingMessage msg = new NetworkingMessage(NetworkingMessageType.PING, clientID);
        return ToByteArray(msg);
    }

    public static byte[] GeneratePingResponseMessage(UInt32 clientID)
    {
        NetworkingMessage msg = new NetworkingMessage(NetworkingMessageType.PING_RESPONSE, clientID);
        return ToByteArray(msg);
    }

    public static byte[] GenerateDisconnectMessage(UInt32 clientID)
    {
        NetworkingMessage msg = new NetworkingMessage(NetworkingMessageType.DISCONNECT, clientID);
        return ToByteArray(msg);
    }


    public static NetworkingMessage ParseMessage(byte[] data)
    {
        return (NetworkingMessage)ByteArrayToObject(data);
    }

    public static int ParseCarModel(byte[] data)
    {
        return BitConverter.ToInt32(data, 0);
    }

    public static NewAccountMsg ParseNewAccountMsg(byte[] data)
    {
        return (NewAccountMsg)ByteArrayToObject(data);
    }

    public static AccountData ParseAccountData(byte[] data)
    {
        return (AccountData)ByteArrayToObject(data);
    }

    public static AccountDataList ParseAccountDataList(byte[] data)
    {
        return (AccountDataList)ByteArrayToObject(data);
    }

    public static SelectedCarData ParseSelectedCarData(byte[] data)
    {
        return (SelectedCarData)ByteArrayToObject(data);
    }
}

[Serializable]
public enum NetworkingMessageType { CLIENT_JOIN, SERVER_JOIN_RESPONSE, DISCONNECT, PING, PING_RESPONSE, GAME_STATE, INPUT_STATE, USER_MANAGER_STATE, CAR_MODEL, TRACK_DATA, NEW_ACCOUNT, NEW_ACCOUNT_RESPONCE, LOGIN, LOGIN_RESPONCE, SAVE_SELECTED_CAR, GLOBAL_LEADERBOARD };

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
public class NewAccountMsg
{
    public ulong accountID;
    public int accountType;

    public NewAccountMsg(ulong accountID, int accountType)
    {
        this.accountID = accountID;
        this.accountType = accountType;
    }
}

[Serializable]
public class AccountData
{
    public ulong accountID;
    public int accountType;
    public string accountName;
    public int coins;
    public int numRaces;
    public int numWins;
    public int selectedCarID;
    public int score;

    public AccountData(ulong accountID, int accountType, string accountName, int coins, int numRaces, int numWins, int selectedCarID, int score)
    {
        this.accountID = accountID;
        this.accountType = accountType;
        this.accountName = accountName;
        this.coins = coins;
        this.numRaces = numRaces;
        this.numWins = numWins;
        this.selectedCarID = selectedCarID;
        this.score = score;
    }
}

[Serializable]
public class AccountDataList
{
    public List<AccountData> accountData = new List<AccountData>();

    public AccountDataList(List<AccountData> accountData)
    {
        this.accountData = accountData;
    }
}

[Serializable]
public class SelectedCarData
{
    public ulong accountID;
    public int accountType;
    public int selectedCarID;

    public SelectedCarData(ulong accountID, int accountType, int selectedCarID)
    {
        this.accountID = accountID;
        this.accountType = accountType;
        this.selectedCarID = selectedCarID;
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