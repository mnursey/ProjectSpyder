using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameRunner : MonoBehaviour
{
    public EntityManager em;
    ServerController sc;
    public PlayerManager pm;
    KillzoneController kzc;

    GameStateEnum state;

    public float timer;
    public float waitingTime = 30f;
    public float endTime = 15f;
    public float zoneMaxSize = 100f;
    public float zoneCloseRate = 0.001f;

    public static int gameStateSendRate = 50;
    public static int playerStateSendRate = 10;
    public static int entityStateSendRate = 4;

    public int frame = 0;

    public static ServerGameRunner Instance;

    public List<GameObject> spawnPositions = new List<GameObject>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogError("More than one Server Game Runner");
        }
    }

    void Start()
    {
        em = GetComponent<EntityManager>();
        pm = GetComponent<PlayerManager>();
        sc = ServerController.Instance;
        kzc = FindObjectOfType<KillzoneController>();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        frame++;

        if(frame % gameStateSendRate == 0)
        {
            SendGameState();
        }

        switch (state)
        {
            // Used for when no players are connected
            case GameStateEnum.IDLE:

                if(AnyPlayersActive())
                {
                    TransitionToWaiting();
                }

                break;

            // Used when waiting for game to start
            case GameStateEnum.WAITING:

                if(!AnyPlayersActive())
                {
                    TransitionToIdle();
                }

                if(timer < 0)
                {
                    TransitionToPlaying();
                }

                if(pm.GetActivePlayerCount() > 1 || true)
                {
                    timer -= Time.deltaTime;
                } else
                {
                    timer = waitingTime;
                }

                break;

            // Used when playing game
            case GameStateEnum.PLAYING:

                GamePlaying();

                if (GameFinished())
                {
                    TransitionToEnd();
                }

                if(!AnyPlayersPlaying())
                {
                    TransitionToIdle();
                }

                break;

            // Used when game is over
            case GameStateEnum.ENDING:

                if (!AnyPlayersActive())
                {
                    TransitionToIdle();
                } else
                {
                    if (timer < 0)
                    {
                        TransitionToWaiting();
                    }
                }

                timer -= Time.deltaTime;

                break;
        }
    }

    void TransitionToWaiting()
    {
        Debug.Log("Transitioned to waiting state");
        state = GameStateEnum.WAITING;
        timer = waitingTime;
        ResetGame();
    }

    void TransitionToIdle()
    {
        Debug.Log("Transitioned to idle state");
        state = GameStateEnum.IDLE;

        if(!AnyPlayersActive())
        {
            pm.Reset();
        }

        ResetGame();
    }

    void TransitionToPlaying()
    {
        Debug.Log("Transitioned to playing state");
        state = GameStateEnum.PLAYING;

        // For all players spawn units
        int i = 0;
        foreach(Player p in pm.players)
        {
            SpawnInitialEntities(p, i % spawnPositions.Count);
            ++i;
        }

        kzc.Init_Server(zoneMaxSize, 0.0f, 1, zoneCloseRate);

        // Todo 
        // Spawn all map entities

        // Send game state to all
        // Send player data to all
        // Send entity data to all
        SendComplete();
    }

    void TransitionToEnd()
    {
        Debug.Log("Transitioned to end state");
        state = GameStateEnum.ENDING;
        timer = endTime;
    }

    void SpawnInitialEntities(Player p, int spawnPos)
    {
        for(int i = 0; i < 5; ++i)
        {
            // TODO
            // Change this to spawn 5 soldiers
            IEntity e = em.CreateEntity(EntityType.MG_JEEP);
            p.controlledEntities.Add(e.id);

            e.gameObject.transform.position = spawnPositions[spawnPos].transform.position + new Vector3(i * 3f, 0f, 0f);
        }
    }

    void GamePlaying()
    {
        DecreaseZone();
        ApplyZoneDamage();

        // Send player update ever n frames
        if (frame % playerStateSendRate == 0)
        {
            SendPlayerState();
        }


        // Send player update ever n frames
        if (frame % entityStateSendRate == 0)
        {
            SendEntityState();
        }


        // Send entity update ever m frames
        if (frame % gameStateSendRate == 0)
        {
            SendPlayerState();
        }
    }

    void SendEntityState()
    {
        // Sends entity state to all clients
        // Entity pos, rot, health, state, ...
        byte[] data;

        data = NetworkingMessageTranslator.GenerateEntityStateNetworkingMessage(em.GetState(), frame);

        // Unreliable since if we miss one it will be updated at the next update
        sc.SendToAll(data, Valve.Sockets.SendFlags.Unreliable);
    } 

    void SendEntityData()
    {
        // Sends entity data to all clients at start of game
        // Everything
        byte[] data;

        data = NetworkingMessageTranslator.GenerateEntityDataNetworkingMessage(em.GetData(), frame);

        // Reliable because we'll only send this on game start, or when a client joins
        sc.SendToAll(data, Valve.Sockets.SendFlags.Reliable);
    }

    void SendPlayerState()
    {
        // Sends player state to all clients
        // Which players control which entites
        byte[] data;

        data = NetworkingMessageTranslator.GeneratePlayerStateNetworkingMessage(pm.GetState(), frame);

        // Reliable because we'll only send this on game start, or when a client joins
        sc.SendToAll(data, Valve.Sockets.SendFlags.Unreliable);
    }

    void SendPlayerData()
    {
        // Sends player data to all clients at start of game
        // Everything
        byte[] data;

        data = NetworkingMessageTranslator.GeneratePlayerDataNetworkingMessage(pm.GetData(), frame);

        // Reliable because we'll only send this on game start, or when a client joins
        sc.SendToAll(data, Valve.Sockets.SendFlags.Reliable);
    }

    void SendGameState()
    {
        // Sends game state plus misc stuff...
        byte[] data;

        float zSize = 0.0f;
        if (state == GameStateEnum.WAITING || state == GameStateEnum.ENDING)
        {
            zSize = timer;
        }
        else
        {
            // send zone size
            zSize = kzc.radius;
        }

        data = NetworkingMessageTranslator.GenerateGameStateNetworkingMessage(new GameState(state, zSize), frame);

        // Reliable because we'll only send this on game start, or when a client joins
        sc.SendToAll(data, Valve.Sockets.SendFlags.Reliable);
    }

    void SendComplete()
    {
        // Sends complete data.. everything
        SendGameState();
        SendEntityData();
        SendPlayerData();
    }

    public void ReceiveUnitCommand(UnitCommand uc)
    {
        IEntity entity = em.GetEntity(uc.entityID);
        IUnit unit = entity.gameObject.GetComponent<IUnit>();

        if (uc.movementUpdate)
        {
            // Update movement
            if (uc.targetWaypoint != null)
            {
                // Movement command
                Vector3 movePosition = uc.targetWaypoint.GetValue();
                unit.SetMoveTarget(uc.targetWaypoint.GetValue());
            }
            else
            {
                // No movement command
                unit.ClearMoveTarget();
            }

        } else
        {
            // Update attack
            IEntity attackTargetEntity = em.GetEntity(uc.attackTargetEntityID);
            Debug.Log("Client Requested to Attack " + uc.attackTargetEntityID);

            if (attackTargetEntity != null)
            {
                IUnit targetUnit = attackTargetEntity.gameObject.GetComponent<IUnit>();
                Debug.Log("Found target unit");
                // Attack command
                unit.SetAttackTarget(targetUnit);
            }
            else
            {
                // No attack command
                unit.ClearAttackTarget();
            }
        }
    }

    void DecreaseZone()
    {
        kzc.DecreaseZone_Server();
    }

    void ApplyZoneDamage()
    {
        kzc.ApplyDamage(em.entities);
    }

    public void ResetGame()
    {
        em.Reset();

        // Reset zone size
        kzc.Init_Server(zoneMaxSize, 0.0f, 1, zoneCloseRate);
    }

    bool GameFinished()
    {
        int numAlivePlayers = 0;

        foreach(Player p in pm.players)
        {
            foreach(ushort u in p.controlledEntities)
            {
                if(em.GetEntity(u) != null && em.GetEntity(u).health > 0)
                {
                    numAlivePlayers++;
                    break;
                }
            }
        }

        if(numAlivePlayers <= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool AnyPlayersActive()
    {
        // Check if any player is online.. Includes idle players
        return pm.GetActivePlayerCount() > 0;
    }

    bool AnyPlayersPlaying()
    {
        // Check if any player is currently playing (Alive or dead)... does not include idle players
        return pm.GetPlayingPlayerCount() > 0;
    }
}
