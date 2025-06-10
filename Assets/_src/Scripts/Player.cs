using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;

public class Player : MonoBehaviourPun
{
    private TMP_Text nicknameTag;
    public GameManagerPhoton GMPhoton;
    public PhotonView view;
    public TablePositions tablePosition;
    public HandCardManager handCardManager;
    public DropZone dropZone;
    public int ActorNumber;
    public string nickname;
    public bool isDealer;
    public bool isRaiseCaller = false;
    public bool isTurn = false;
    public bool canPlay = false;
    public bool isFoot = false;
    void Start()
    {
        GMPhoton = GameObject.Find("GameManagerPhoton").GetComponent<GameManagerPhoton>();
        nicknameTag = transform.Find("NicknameTag")?.GetComponent<TMP_Text>();
        StartCoroutine(DelayedStart());
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        tablePosition = GetComponent<TablePositions>();
        view = GetComponent<PhotonView>();
        Transform spawnPosition = GetViewPosition(view);
        dropZone = spawnPosition.transform.Find("DropZone").GetComponent<DropZone>();
        transform.SetParent(spawnPosition);
        transform.position = spawnPosition.position;
        handCardManager.Initialize(this);
        nicknameTag.text = view.Controller.NickName;
        nickname = view.Controller.NickName;
        PhotonView pv = GetComponent<PhotonView>();
        ActorNumber = pv.Owner.ActorNumber;
        GMPhoton.AddPlayerObj(this);
    }
    public Transform GetViewPosition(PhotonView playerView)
    {
        return playerView.IsMine ? tablePosition.clientPlayerTransform : tablePosition.remotePlayerTransform.transform;
    }
}
