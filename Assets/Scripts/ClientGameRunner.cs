using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateEnum { IDLE, JOINING, WAITING, PLAYING, ENDING }

public class ClientGameRunner : MonoBehaviour
{
    EntityManager em;
    PlayerManager pm;
    ClientController cc;

    GameStateEnum state;

    int lastEntityDataUpdateFrame = 0;
    int lastPlayerDataUpdateFrame = 0;

    int lastEntityStateUpdateFrame = 0;
    int lastPlayerStateUpdateFrame = 0;
    int lastGameStateUpdateFrame = 0;

    GameState incomingGameState;
    List<PlayerState> incomingPlayerState;
    List<PlayerData> incomingPlayerData;
    List<EntityState> incomingEntityState;
    List<EntityData> incomingEntityData;

    public uint playerID;
    public string username;

    // Start is called before the first frame update
    void Start()
    {
        em = GetComponent<EntityManager>();
        pm = GetComponent<PlayerManager>();
        cc = ClientController.Instance;
        ConnectToGame();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameState();
        UpdateEntities();
        UpdatePlayers();

        switch (state)
        {
            // Used for main menu
            case GameStateEnum.IDLE:
                break;

            // Used when joining server
            case GameStateEnum.JOINING:
                break;

            // Used when spectating on going game
            case GameStateEnum.WAITING:
                break;

            // Used when playing game... Dead or alive
            case GameStateEnum.PLAYING:

                // TODO
                // CAMERA
                // SEND COMMANDS

                break;

            // Used when game is over
            case GameStateEnum.ENDING:
                break;
        }
    }

    void ConnectToGame()
    {
        cc.JoinGame(username, OnConnect, OnDisconnect, OnReject);
        // TODO
    }

    void OnConnect(bool connected)
    {
        if(connected)
        {
            // Send username
            SendUsername();
        } else
        {
            Debug.Log("Could not connect to server");
            // Show could not connect UI
            // Reset
        }
    }

    void OnDisconnect()
    {
        Debug.Log("Disconnected from server");
        // Show disconnected UI
        // Reset
    }

    void OnReject(string reason)
    {
        Debug.Log("Rejected from server: " + reason);
        // Show rejected reason UI
        // Reset
    }

    void SendCommands()
    {

    }

    void SendUsername()
    {
        // Sends username to the server
        byte[] data;

        data = NetworkingMessageTranslator.GenerateUsernameNetworkingMessage(username);

        // Reliable because we only send once.
        cc.Send(data, Valve.Sockets.SendFlags.Reliable, null);
    }

    void MoveCommand()
    {
        // TODO
    }

    void AttackCommand()
    {
        // TODO
    }

    void StopCommand()
    {
        // TODO
    }

    void EnterCommand()
    {
        // TODO
    }

    void QuitGame()
    {
        // TODO
    }

    public void ReceiveEntityState(List<EntityState> es, int frame)
    {
        if(incomingEntityState == null || lastEntityStateUpdateFrame < frame)
        {
            lastEntityStateUpdateFrame = frame;
            incomingEntityState = es;
        }
    }

    public void ReceivePlayerState(List<PlayerState> ps, int frame)
    {
        if (incomingPlayerState == null || lastPlayerStateUpdateFrame < frame)
        {
            lastPlayerStateUpdateFrame = frame;
            incomingPlayerState = ps;
        }
    }

    public void ReceiveGameState(GameState gs, int frame)
    {
        if (incomingGameState == null || lastGameStateUpdateFrame < frame)
        {
            lastGameStateUpdateFrame = frame;
            incomingGameState = gs;
        }
    }

    public void ReceiveEntityData(List<EntityData> ed, int frame)
    {
        if (incomingEntityData == null || lastEntityDataUpdateFrame < frame)
        {
            lastEntityDataUpdateFrame = frame;
            incomingEntityData = ed;
        }
    }

    public void ReceivePlayerData(List<PlayerData> pd, int frame)
    {
        if (incomingPlayerData == null || lastPlayerDataUpdateFrame < frame)
        {
            lastPlayerDataUpdateFrame = frame;
            incomingPlayerData = pd;
        }
    }

    public void UpdateEntities()
    {
        if (lastEntityDataUpdateFrame < lastEntityStateUpdateFrame)
        {
            // Data first...
            if (incomingEntityData != null)
            {
                em.SetData(incomingEntityData);
                incomingEntityData = null;
            }

            // Then state
            em.SetState(incomingEntityState);
        }
        else
        {
            // Just data
            if (incomingEntityData != null)
            {
                em.SetData(incomingEntityData);
                incomingEntityData = null;
            }
        }
    }

    public void UpdatePlayers()
    {
        if(lastPlayerDataUpdateFrame < lastPlayerStateUpdateFrame)
        {
            // Data first...
            if(incomingPlayerData != null)
            {
                pm.SetData(incomingPlayerData);
                incomingPlayerData = null;
            }

            // Then state
            pm.SetState(incomingPlayerState);
        } else
        {
            // Just data
            if (incomingPlayerData != null)
            {
                pm.SetData(incomingPlayerData);
                incomingPlayerData = null;
            }
        }
    }

    public void UpdateGameState()
    {
        // TODO
    }

    public void ResetGame()
    {
        em.Reset();

        lastEntityStateUpdateFrame = 0;
        lastPlayerStateUpdateFrame = 0;
        lastGameStateUpdateFrame = 0;

        lastEntityDataUpdateFrame = 0;
        lastPlayerDataUpdateFrame = 0;

        incomingGameState = null;
        incomingPlayerState = null;
        incomingPlayerData = null;
        incomingEntityState = null;
        incomingEntityData = null;
    }

    public void Reset()
    {
        ResetGame();
        playerID = 0;
        state = GameStateEnum.IDLE;
        pm.Reset();
    }
}
