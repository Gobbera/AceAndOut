using UnityEngine;
using Photon.Pun;

public enum GameState { WAITING_PLAYERS, READY_TO_PLAY, DEALING_CARDS }

public class GameManagerPhoton : MonoBehaviourPun
{
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

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Iniciar com o estado de espera dos jogadores
            UpdateGameState(GameState.WAITING_PLAYERS);
        }
    }

    // Método para atualizar o estado do jogo
    [PunRPC]
    public void UpdateGameState(GameState newState)
    {
        gameState = newState;
        // Log para depuração
        Debug.Log("Novo estado do jogo: " + gameState);

        // Baseado no novo estado, você pode iniciar novas ações
        switch (newState)
        {
            case GameState.WAITING_PLAYERS:
                // Aqui você pode colocar lógica adicional quando o estado for "WAITING_PLAYERS"
                break;
            case GameState.READY_TO_PLAY:
                // Aqui você pode iniciar a lógica para o jogo começar
                break;
            case GameState.DEALING_CARDS:
                // Aqui você pode começar a distribuir as cartas
                break;
        }
    }
}
