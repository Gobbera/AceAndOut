using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;


public class GameController : MonoBehaviourPunCallbacks
{
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
            addPlayer();
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            addPlayer();
        }
    }
    void addPlayer()
    {
            Player player = new Player();
            players.Add(player);
    }
    public void SetPlayersHands(HandCardManager hand) 
    { 
        playerHands.Add(hand); 
    }
}
    
