using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameStateEnum { IDLE, JOINING, WAITING, PLAYING, ENDING }

public class ClientGameRunner : MonoBehaviour
{
    public EntityManager em;
    PlayerManager pm;
    ClientController cc;
    KillzoneController kzc;

    public GameStateEnum state;

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
    public Text usernameField;

    public static ClientGameRunner Instance;

    private void Awake()
    {
        if(Instance == null )
        {
            Instance = this;
        } else
        {
            Debug.LogError("More than one Client Game Runner");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        em = GetComponent<EntityManager>();
        pm = GetComponent<PlayerManager>();
        cc = ClientController.Instance;
        kzc = FindObjectOfType<KillzoneController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameState();
        UpdateEntities();
        UpdatePlayers();
        UpdateEntityOutlineColour();

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

    void TransitionToIdle()
    {
        Debug.Log("Transitioned to idle state");
        state = GameStateEnum.IDLE;

    }

    void TransitionToWaiting()
    {
        Debug.Log("Transitioned to waiting state");
        state = GameStateEnum.WAITING;
        MenuController.Instance.GoToWaitMenu();
        ResetGame();
    }

    void TransitionToJoining()
    {
        Debug.Log("Transitioned to joining state");
        state = GameStateEnum.JOINING;

    }

    void TransitionToPlaying()
    {
        Debug.Log("Transitioned to playing state");
        state = GameStateEnum.PLAYING;

        kzc.Init_Client(10000f, 0.0f, 1, 0f);

        // Close all menus
        MenuController.Instance.CloseMenu();
    }

    void TransitionToEnding()
    {
        Debug.Log("Transitioned to ending state");
        state = GameStateEnum.ENDING;
        MenuController.Instance.GoToEndMenu();

    }

    public void ConnectToGame()
    {
        Reset();
        cc.JoinGame(usernameField.text, OnConnect, OnDisconnect, OnReject);
        TransitionToJoining();

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
            MenuController.Instance.GoToConnectionErrorMenu();
            MenuController.Instance.SetConnectionErrorReason("Could not connect to server...\nCheck your connection...\nCheck if server is running...");

            LeaveServer();
        }
    }

    void OnDisconnect()
    {
        Debug.Log("Disconnected from server");
        // Show disconnected UI
        MenuController.Instance.GoToConnectionErrorMenu();
        MenuController.Instance.SetConnectionErrorReason("Disconnected from server");

        LeaveServer();
    }

    void OnReject(string reason)
    {
        Debug.Log("Rejected from server: " + reason);
        // Show rejected reason UI

        MenuController.Instance.GoToConnectionErrorMenu();
        MenuController.Instance.SetConnectionErrorReason("Rejected from joining server.\n Reason:\n" + reason);

        LeaveServer();
    }

    public bool IsOurUnit(GameObject unit)
    {
        bool result = false;

        IEntity entity = em.GetEntity(unit);

        if(entity != null)
        {
            result = pm.GetPlayer(playerID).controlledEntities.Contains(entity.id);
        }

        return result;
    }

    void UpdateEntityOutlineColour()
    {
        foreach(IEntity e in em.entities)
        {
            Outline o = e.gameObject.GetComponent<Outline>();

            if(o != null)
            {
                uint ownerPlayerID = 0;
                foreach(Player p in pm.players)
                {
                    if(p.controlledEntities.Contains(e.id))
                    {
                        ownerPlayerID = p.id;
                        break;
                    }
                }

                if(ownerPlayerID == 0)
                {
                    // neutral
                    o.OutlineColor = Color.blue;
                } else if(ownerPlayerID == playerID)
                {
                    // ours
                    // let player controls handle this case...
                    // Todo... redo this ^ 

                    // Hacky fix...
                    // This fixes the case where the units are spawned then the CGR receives its playerID
                    if (o.OutlineColor == Color.blue)
                    {
                        o.OutlineColor = Color.white;
                    }
                }
                else
                {
                    // enemy
                    o.OutlineColor = Color.red;
                }
            }
        }
    }

    void SendUsername()
    {
        // Sends username to the server
        byte[] data;

        data = NetworkingMessageTranslator.GenerateUsernameNetworkingMessage(usernameField.text);

        // Reliable because we only send once.
        cc.Send(data, Valve.Sockets.SendFlags.Reliable, null);
    }

    public void IssueComand(IUnit unit, Vector3 moveTarget, bool moveTargetActive)
    {
        // HANDLE MOVEMENT
        ushort entityID = ClientGameRunner.Instance.em.GetEntity(unit.GetGameObject()).id;

        SVector3 mt = null;

        if(moveTargetActive)
        {
            mt = new SVector3(moveTarget);
        }

        UnitCommand uc = new UnitCommand(entityID, mt);

        SendUnitCommand(uc);
    }

    public void IssueComand(IUnit unit, IUnit attackTarget)
    {
        // HANDLE ATTACK
        ushort entityID = ClientGameRunner.Instance.em.GetEntity(unit.GetGameObject()).id;
        ushort attackTargetEntityID = 0;

        // TODO
        // Refactor this null checking
        if (attackTarget != null && attackTarget.GetGameObject() != null && ClientGameRunner.Instance.em.GetEntity(attackTarget.GetGameObject()) != null)
        {
            attackTargetEntityID = ClientGameRunner.Instance.em.GetEntity(attackTarget.GetGameObject()).id;
        }

        Debug.Log("Attacking " + attackTargetEntityID);

        UnitCommand uc = new UnitCommand(entityID, attackTargetEntityID);

        SendUnitCommand(uc);
    }

    public void SendUnitCommand(UnitCommand uc)
    {
        byte[] data;

        data = NetworkingMessageTranslator.GenerateUnitCommandNetworkingMessage(uc);

        // Reliable because we only send once.
        cc.Send(data, Valve.Sockets.SendFlags.Reliable, null);
    }

    public void ReceiveEntityState(List<EntityState> es, int frame)
    {
        if(incomingEntityState == null || lastEntityStateUpdateFrame < frame)
        {
            lastEntityStateUpdateFrame = frame;
            incomingEntityState = es;
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
        if(incomingGameState != null && state != incomingGameState.state)
        {
            // Transition to new state

            switch(incomingGameState.state)
            {
                case GameStateEnum.IDLE:
                    ResetGame();
                    break;

                case GameStateEnum.JOINING:
                    Debug.LogError("Client should never receive joining state");
                    break;

                case GameStateEnum.PLAYING:
                    TransitionToPlaying();
                    break;

                case GameStateEnum.WAITING:
                    TransitionToWaiting();
                    break;

                case GameStateEnum.ENDING:
                    TransitionToEnding();
                    break;
            }

            if(incomingGameState != null)
                state = incomingGameState.state;
        }

        if(incomingGameState != null)
        {
            switch (incomingGameState.state)
            {
                case GameStateEnum.WAITING:
                    MenuController.Instance.SetWaitingText("Game starting in " + Mathf.RoundToInt(incomingGameState.zoneSize) + " seconds");
                    break;

                case GameStateEnum.ENDING:
                    MenuController.Instance.SetEndingText("Next game in " + Mathf.RoundToInt(incomingGameState.zoneSize) + " seconds");
                    break;

                case GameStateEnum.PLAYING:
                    kzc.SetZone_Client(incomingGameState.zoneSize);
                    break;
            }
        }
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
        TransitionToIdle();
        pm.Reset();

        incomingGameState = null;

        incomingEntityState = null;
        incomingPlayerState = null;

        incomingPlayerData = null;
        incomingEntityData = null;
    }

    public void LeaveServer()
    {
        cc.Disconnect();
        Reset();
    }
}

[Serializable]
public class UnitCommand
{
    public ushort entityID;

    public ushort attackTargetEntityID;

    public SVector3 targetWaypoint;

    public bool movementUpdate = true;

    public UnitCommand(ushort entityID, SVector3 targetWaypoint)
    {
        this.entityID = entityID;
        this.targetWaypoint = targetWaypoint;
    }

    public UnitCommand(ushort entityID, ushort attackTargetEntityID)
    {
        this.entityID = entityID;
        this.attackTargetEntityID = attackTargetEntityID;
        movementUpdate = false;
    }
}
