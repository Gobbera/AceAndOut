using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;

public enum GameState { WAITING_PLAYERS, READY_TO_PLAY, STARTING_THE_GAME, DEALING_CARDS }

public class GameManagerPhoton : MonoBehaviourPunCallbacks
{
    public GameController gameController;
    public List<Player> players = new List<Player>();
    public List<HandCardManager> playerHands = new List<HandCardManager>();
    public int playerCount = 0;
    public static GameManagerPhoton Instance;
    public GameState gameState;
    public Deck deck;
    public Dealer dealer;
    public int cardsPerPlayer = 3;
    public List<CardData> cardsDataOnGame = new List<CardData>();
    public bool hasChanges = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
            ChangeState(GameState.WAITING_PLAYERS);
        }
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && hasChanges)
        {
            hasChanges = false; // Reseta após processar a alteração
            
            if (playerCount >= 2)
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
    private void ChangeState(GameState newState)
    {
        if (PhotonNetwork.IsMasterClient) 
        {
            photonView.RPC("UpdateGameState", RpcTarget.AllBuffered, (int)newState);
        }
    }
    [PunRPC]
    public void UpdateGameState(int newStateInt)
    {
        gameState = (GameState)newStateInt;
        Debug.Log($"[GameManagerPhoton] Novo estado do jogo: {gameState}");

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
                DealCards();
                break;
        }
    }
    public void AddPlayerObj(Player player)
    {
        if (player)
        {
            players.Add(player.GetComponent<Player>());
        }
        if (player.handCardManager != null)
        {
                playerHands.Add(player.handCardManager.GetComponent<HandCardManager>());
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InstanceRemotePlayer(newPlayer);
        }
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        /* if (PhotonNetwork.IsMasterClient)
        {
            Instance = this; // O novo MasterClient assume o GameManager
        } */
    }
    public void InstanceRemotePlayer(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && !hasChanges)
        {
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            NotifyChange();
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
        if (PhotonNetwork.IsMasterClient && !hasChanges)
        {
            hasChanges = true;
            Debug.Log("[GameManagerPhoton] Notificação de mudança enviada.");
        }
    }
    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int playersCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int totalCardsNeeded = cardsPerPlayer * playersCount;

        if (deck.GetCards().Count < totalCardsNeeded)
        {
            Debug.LogWarning("[GameManagerPhoton] Deck não possui cartas suficientes.");
            return;
        }
        int[] shuffledDeck = deck.ShuffleAndSyncDeck();
        photonView.RPC("ReceiveShuffledDeck", RpcTarget.OthersBuffered, shuffledDeck);
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList)
        {
            photonView.RPC("ReceiveCards", photonPlayer, photonPlayer.ActorNumber);
        }
    }
    [PunRPC]
    public void ReceiveCards(int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            { 
                playerHands[0].AddCard(deck.availableCards[0]);
                playerHands[1].AddHiddenCard();                                 //Temporario
                photonView.RPC("RemoveCard", RpcTarget.AllBuffered);
            }
        }
    }
    [PunRPC]
    public void RemoveCard() 
    {
        deck.RemoveCardData();
    }
    [PunRPC]
    public void ReceiveShuffledDeck(int[] shuffledDeck) 
    {
        deck.ReceiveShuffledDeck(shuffledDeck);
    }
}
