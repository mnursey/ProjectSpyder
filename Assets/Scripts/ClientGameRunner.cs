using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GameStateEnum { IDLE, JOINING, WAITING, PLAYING, ENDING }

public class ClientGameRunner : MonoBehaviour
{
    EntityManager em;
    ClientController cc;

    GameStateEnum state;

    public Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        //em = GetComponent<EntityManager>();
        //cc = ClientController.Instance;
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();

        state = GameStateEnum.PLAYING;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
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
                // UPDATE GAME STATE / VISUALS

               

                break;

            // Used when game is over
            case GameStateEnum.ENDING:
                break;
        }
    }

    void ConnectToGame()
    {
        // TODO
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

    public void SetEntityState()
    {
        // TODO
    }

    public void SetPlayerState()
    {
        // TODO
    }

    public void SetGameState()
    {
        // TODO
    }

    public void UpdateEntityState()
    {
        // TODO
    }

    public void UpdatePlayerState()
    {
        // TODO
    }

    public void ResetGame()
    {
        em.Reset();
    }
}
