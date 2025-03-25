using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class PlayerSpawn : MonoBehaviour
{
    public GameController gameController;
    public GameObject playerPrefab;
    public GameObject DropZone;
    public Transform spawnPosition;
    
    void Start()
    {
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition.position, Quaternion.identity);

        playerObject.transform.SetParent(spawnPosition);
        Player player = playerObject.GetComponent<Player>();

        DropZone dropZone = spawnPosition.GetComponentInChildren<DropZone>();
        player.dropZone = dropZone;
        dropZone.dropZoneName = player.nickname;

        gameController.SetPlayersHands(player.handCardManager);
    }
}
