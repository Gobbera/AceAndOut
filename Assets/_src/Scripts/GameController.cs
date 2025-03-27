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
    [SerializeField] private List<HandCardManager> playerHands = new List<HandCardManager>();
    public GameObject playerPrefab;
    public GameObject clientViewPosition;
    public GameObject enemyViewPosition;
    public int cardsPerPlayer = 3;
    public bool hasChanges;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Eu SOU o masterClient");
        }
        else
        {
            Debug.Log("Eu NÃO SOU o masterClient");
        }
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.LocalPlayer.IsLocal)
            {
                addClientPlayer();
                GMPhoton.NotifyChange();
            }
        }

    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            addEnemyPlayer(newPlayer);
            GMPhoton.NotifyChange();
        }
    }
    void addClientPlayer()
    {
        GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, clientViewPosition.transform.position, Quaternion.identity);
        Player playerComponent = localPlayer.GetComponent<Player>();
        playerComponent.ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        playerComponent.nickname = PhotonNetwork.NickName;
        players.Add(playerComponent);
        localPlayer.transform.SetParent(clientViewPosition.transform);
        GMPhoton.NotifyChange();
    }
    void addEnemyPlayer(Photon.Realtime.Player newPlayer)
    {
        GameObject remotePlayer = PhotonNetwork.Instantiate(playerPrefab.name, enemyViewPosition.transform.position, Quaternion.identity);
        Player playerComponent = remotePlayer.GetComponent<Player>();
        playerComponent.ActorNumber = newPlayer.ActorNumber;
        playerComponent.nickname = newPlayer.NickName;
        players.Add(playerComponent);
        remotePlayer.transform.SetParent(enemyViewPosition.transform);
        GMPhoton.NotifyChange();
    }
    public void DealCards()
    {
        dealer.DealCards(cardsPerPlayer, deck, playerHands);
    }
    public void SetPlayersHands(HandCardManager hand) 
    { 
        playerHands.Add(hand); 
    }
}
    
