using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Player : MonoBehaviour
{
    public PhotonView view;
    public TablePositions tablePosition;
    public HandCardManager handCardManager;
    public DropZone dropZone;
    public int ActorNumber;
    public string nickname;
    public bool isDealer;
    public bool isTurn;
    void Start()
    {
        StartCoroutine(DelayedStart());
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        tablePosition = GetComponent<TablePositions>();
        view = GetComponent<PhotonView>();
        Transform spawnPosition = GetViewPosition(view);
        transform.SetParent(spawnPosition);
        transform.position = spawnPosition.position;
        handCardManager.Initialize(this);
    }
    public Transform GetViewPosition(PhotonView playerView)
    {
        return playerView.IsMine ? tablePosition.clientPlayerTransform : tablePosition.remotePlayerTransform.transform;
    }
}
