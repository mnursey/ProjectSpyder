using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameRunner : MonoBehaviour
{
    public EntityManager em;
    ServerController sc;
    public PlayerManager pm;

    GameStateEnum state;

    public float timer;
    public float waitingTime = 30f;
    public float endTime = 15f;
    public float zoneMaxSize = 100f;
    public float zoneCloseRate = 0.001f;

    public int gameStateSendRate = 50;
    public int playerStateSendRate = 10;
    public int entityStateSendRate = 4;

    public int frame = 0;

    void Start()
    {
        em = GetComponent<EntityManager>();
        pm = GetComponent<PlayerManager>();
        sc = ServerController.Instance;
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

                if(pm.GetPlayerCount() > 1 || true)
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
        foreach(Player p in pm.players)
        {
            SpawnInitialEntities(p);
        }

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

    void SpawnInitialEntities(Player p)
    {
        for(int i = 0; i < 5; ++i)
        {
            // TODO
            // Change this to spawn 5 soldiers
            IEntity e = em.CreateEntity(EntityType.JEEP);
            p.controlledEntities.Add(e.id);

            // TODO Set positions of units to a spawn position
            e.gameObject.transform.position = new Vector3(i * 10f, 0f, 0f);
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
            // Todo send actual zone size
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

    void DecreaseZone()
    {
        // Todo
        // decrease radius of death zone
    }

    void ApplyZoneDamage()
    {
        // Todo
    }

    public void ResetGame()
    {
        frame = 0;
        em.Reset();
        
        // Todo
        // Reset zone size
    }

    bool GameFinished()
    {
        // TODO
        // Check if one player has units alive
        return false;
    }

    bool AnyPlayersActive()
    {
        // Check if any player is online.. Includes idle players
        return pm.GetPlayerCount() > 0;
    }

    bool AnyPlayersPlaying()
    {
        // Check if any player is currently playing (Alive or dead)... does not include idle players
        return pm.GetPlayingPlayerCount() > 0;
    }
}
