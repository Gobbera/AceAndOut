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
    public int ActorNumber;
    public string nickname;
    public bool isDealer;
    public bool isTurn;
    void Start()
    {
        view = GetComponent<PhotonView>();
            
        handCardManager.Initialize(this);
    }
}
