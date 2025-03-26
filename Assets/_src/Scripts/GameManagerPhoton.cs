using UnityEngine;
using Photon.Pun;
using System.Collections;

public enum GameState { WAITING_PLAYERS, READY_TO_PLAY, STARTING_THE_GAME, DEALING_CARDS }

public class GameManagerPhoton : MonoBehaviourPun
{
    public GameController gameController;
    public static GameManagerPhoton Instance;
    public GameState gameState;
    
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

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameController.players.Count >= 2 && gameState == GameState.WAITING_PLAYERS)
            {
                photonView.RPC("UpdateGameState", RpcTarget.AllBuffered, GameState.READY_TO_PLAY);
            }
        }
    }
    
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdateGameState", RpcTarget.AllBuffered, GameState.WAITING_PLAYERS);
        }
    }

    [PunRPC]
    public void UpdateGameState(GameState newState)
    {
        gameState = newState;
        Debug.Log("Novo estado do jogo: " + gameState);

        switch (newState)
        {
            case GameState.WAITING_PLAYERS:
                gameController.gameLog.changeText("Aguardando Jogadores...");
                break;
            case GameState.READY_TO_PLAY:
                gameController.gameLog.changeText("Adversário encontrado! O jogo vai iniciar em instantes...");
                break;
            case GameState.STARTING_THE_GAME:
                StartCoroutine(gameController.gameLog.StartCountdown());
                photonView.RPC("UpdateGameState", RpcTarget.AllBuffered, GameState.DEALING_CARDS);
                break;
            case GameState.DEALING_CARDS:
                gameController.gameLog.changeText("Distribuindo Cartas...");
                break;
        }
    }
}
