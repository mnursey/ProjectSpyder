using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerGameRunner : MonoBehaviour
{
    EntityManager em;
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

                if(pm.GetPlayerCount() > 1)
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
                }

                if (timer < 0)
                {
                    TransitionToIdle();
                }

                timer -= Time.deltaTime;

                break;
        }
    }

    void TransitionToWaiting()
    {
        state = GameStateEnum.WAITING;
        timer = waitingTime;
    }

    void TransitionToIdle()
    {
        state = GameStateEnum.IDLE;
    }

    void TransitionToPlaying()
    {
        state = GameStateEnum.PLAYING;
        // Todo
        // For all players spawn units

        // Send game state to all
        // Send player data to all
        // Send entity data to all
        SendComplete();
    }

    void TransitionToEnd()
    {
        state = GameStateEnum.ENDING;
        timer = endTime;
    }

    void GamePlaying()
    {
        CloseZone();

        // Send player update ever n frames
        if (frame % playerStateSendRate == 0)
        {
            SendPlayerState();
        }

        // Send entity update ever m frames
        if (frame % gameStateSendRate == 0)
        {
            SendPlayerState();
        }
    }

    void SendEntityState()
    {
        // Todo
        // Sends entity state to all clients
        // Entity pos, rot, health, state, ...
    } 

    void SendPlayerState()
    {
        // Todo
        // Sends player state to all clients
        // Which players control which entites
    }

    void SendEntityData()
    {
        // Todo
        // Sends entity data to all clients at start of game
        // Everything
    }

    void SendPlayerData()
    {
        // Todo
        // Sends player data to all clients at start of game
        // Everything
    }

    void SendGameState()
    {
        // Todo
        // Sends game state plus misc stuff...
    }

    void SendComplete()
    {
        // Sends complete data.. everything
        SendGameState();
        SendEntityData();
        SendPlayerData();
    }

    void CloseZone()
    {
        // Todo
        // decrease radius of death zone
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
