using UnityEngine;
using Photon.Pun;
using System.Collections;

public enum GameState { WAITING_PLAYERS, READY_TO_PLAY, STARTING_THE_GAME, DEALING_CARDS }

public class GameManagerPhoton : MonoBehaviourPun
{
    public GameController gameController;
    public static GameManagerPhoton Instance;
    public GameState gameState;
    public bool hasChanges = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && hasChanges)
        {
            hasChanges = false; // Reseta após processar a alteração
            
            if (gameController.players.Count >= 2)
            {
                switch (gameState)
                {
                    case GameState.WAITING_PLAYERS:
                        ChangeState(GameState.READY_TO_PLAY);
                        break;
                    case GameState.READY_TO_PLAY:
                        ChangeState(GameState.STARTING_THE_GAME);
                        break;
                    case GameState.STARTING_THE_GAME:
                        ChangeState(GameState.DEALING_CARDS);
                        break;
                }
            }
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeState(GameState.WAITING_PLAYERS);
        }
    }

    private void ChangeState(GameState newState)
    {
        photonView.RPC("UpdateGameState", RpcTarget.AllBuffered, (int)newState);
    }

    [PunRPC]
    public void UpdateGameState(int newStateInt)
    {
        gameState = (GameState)newStateInt;
        Debug.Log("Novo estado do jogo: " + gameState);

        switch (gameState)
        {
            case GameState.WAITING_PLAYERS:
                gameController.gameLog.changeText("Aguardando Jogadores...");
                break;
            case GameState.READY_TO_PLAY:
                gameController.gameLog.changeText("Adversário encontrado! O jogo vai iniciar em instantes...");
                NotifyChange();
                break;
            case GameState.STARTING_THE_GAME:
                StartCoroutine(StartCountdown());
                break;
            case GameState.DEALING_CARDS:
                gameController.gameLog.changeText("Distribuindo Cartas...");
                gameController.DealCards();
                break;
        }
    }

    private IEnumerator StartCountdown()
    {
        yield return gameController.gameLog.StartCountdown();
        ChangeState(GameState.DEALING_CARDS);
        NotifyChange();
    }

    public void NotifyChange()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            hasChanges = true;
        }
    }
}
