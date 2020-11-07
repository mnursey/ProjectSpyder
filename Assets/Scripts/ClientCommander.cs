using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCommander : MonoBehaviour
{
    // Misc

    public string username = "TestUsername";

    // References
    ClientController cc;

    void Start()
    {
        cc = ClientController.Instance;
        OnConnectedToServer();
    }

    void Update()
    {
        
    }

    public void OnConnectedToServer()
    {
        cc.ConnectToServer("mnursey", OnConnect, OnDisconnect, OnReject);
    }

    void OnConnect (bool connected)
    {
        Debug.Log("Connected to server " + connected.ToString());

        if(connected)
        {
            GameState gs = new GameState(username);
            byte[] data = NetworkingMessageTranslator.GenerateGameStateNetworkingMessage(gs);
            cc.Send(data, Valve.Sockets.SendFlags.Reliable, null);
        }
    }

    void OnReject(string reason)
    {
        Debug.Log("Rejected from server: " + reason);
    }

    void OnDisconnect ()
    {
        Debug.Log("Disconnected from server");
    }
}
