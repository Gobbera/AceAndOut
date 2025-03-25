using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks 
{
    public InputField createInput;
    public InputField joinInput;
    public InputField NicknameInput;

    public void JoinRoom()
    {
        PhotonNetwork.NickName = NicknameInput.text;
        PhotonNetwork.JoinRoom(joinInput.text);
    }
    public void CreateRoom()
    {
        PhotonNetwork.NickName = NicknameInput.text;
        PhotonNetwork.CreateRoom(createInput.text);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LoadLevel("Game");
            }
        }
    }
}
