using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MatchDataManager : MonoBehaviourPunCallbacks
{
    public static MatchDataManager Instance;
    public int playerCount = 0;   // Número de jogadores na sala
    public int roundNumber = 0;   // Número da rodada

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantém esse objeto entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            CheckStartGame();
        }
    }

    // Sobrescreve o método de callback que é chamado quando um jogador entra na sala
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) 
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log($"Jogador entrou! Total de jogadores: {playerCount}");
        CheckStartGame();
    }

    // Sobrescreve o método de callback que é chamado quando um jogador sai da sala
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log($"Jogador saiu! Total de jogadores: {playerCount}");
    }

    private void CheckStartGame()
    {
        // O jogo só pode começar se o MasterClient e houver mais de 2 jogadores
        if (PhotonNetwork.IsMasterClient && playerCount > 2)
        {
            Debug.Log("🔹 Múltiplos jogadores detectados! Iniciando jogo...");
            // Enviar RPC para todos os jogadores para atualizar o estado do jogo
            GameManagerPhoton.Instance.photonView.RPC("UpdateGameState", RpcTarget.All, GameState.READY_TO_PLAY);
        }
    }
}
