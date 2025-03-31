using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawn : MonoBehaviour
{
    public GameManagerPhoton GMPhoton;
    public GameObject playerPrefab;
    void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        GMPhoton.players.Add(player.GetComponent<Player>());
        GMPhoton.playerHands.Add(player.GetComponent<HandCardManager>());
    }
}
