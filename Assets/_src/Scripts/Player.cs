using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Player : MonoBehaviour
{
    public PhotonView view;
    public HandCardManager handCardManager;
    public DropZone dropZone;
    public int playerIndex;
    public string nickname;
    public bool isDealer;
    public bool isTurn;
    void Start()
    {
        view = GetComponent<PhotonView>();
        
        nickname = PhotonNetwork.LocalPlayer.NickName;
            
        handCardManager.Initialize(this);
    }
}
