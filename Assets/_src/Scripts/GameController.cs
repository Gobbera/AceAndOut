using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;


public class GameController : MonoBehaviourPunCallbacks
{
    public GameManagerPhoton GMPhoton;
    public GameLog gameLog;
    public Deck deck;
    public Dealer dealer;
    [SerializeField] public List<Player> players = new List<Player>();
    [SerializeField] public List<HandCardManager> playerHands = new List<HandCardManager>();
    public GameObject playerPrefab;
    public int cardsPerPlayer = 3;
    public bool hasChanges;

    public void DealCards()
    {
        dealer.DealCards(cardsPerPlayer, deck, GMPhoton.playerHands);
    }
}
    
