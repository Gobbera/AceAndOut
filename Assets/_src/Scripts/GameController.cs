using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;


public class GameController : MonoBehaviourPunCallbacks
{
    public GameManager GameManager;
    public GameLog gameLog;
    public Deck deck;
    public Dealer dealer;
    [SerializeField] public List<Player> players = new List<Player>();
    [SerializeField] private List<HandCardManager> playerHands = new List<HandCardManager>();
    public GameObject playerPrefab;
    public int cardsPerPlayer = 3;
    public bool hasChanges;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Eu SOU o masterClient");
        } else {
            Debug.Log("Eu NÃO SOU o masterClient");
        }
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Player player = new Player();
            players.Add(player);
            hasChanges = true;
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player player = new Player();
            players.Add(player);
            hasChanges = true;
        }
    }
    void Update()
    {   
        if (!hasChanges) return;
        if (players.Count < 2) GameGameState(GameState.WAITING_PLAYERS);
        if (players.Count >= 2) GameGameState(GameState.READY_TO_PLAY);
        hasChanges = false;
    }


    public void GameGameState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.WAITING_PLAYERS:
                gameLog.changeText("Aguardando Jogadores");
                break;
            case GameState.READY_TO_PLAY:
                gameLog.changeText("Iniciando Jogo");
                StartGame();
                break;
            default:
                break;
        }
    }

    public void SetPlayer(Player player) 
    { 
        hasChanges = true;
    }

    public void SetPlayersHands(HandCardManager hand) 
    { 
        playerHands.Add(hand); 
    }
    
    private void StartGame()
    {
        dealer.DealCards(cardsPerPlayer, deck, playerHands);
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.StartGame(players);
        }
    }
}
