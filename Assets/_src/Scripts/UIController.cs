using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] public GameManagerPhoton GMPhoton;
    [SerializeField] public UIButtons trucoButton;
    [SerializeField] public UIButtons acceptButton;
    [SerializeField] public UIButtons runButton;
    [SerializeField] public UIButtons raiseButton;
    void Awake()
    {
        trucoButton.button.onClick.AddListener(OnButtonTrucoClick);
        acceptButton.button.onClick.AddListener(OnButtonAcceptClick);
        runButton.button.onClick.AddListener(OnButtonRunClick);
        raiseButton.button.onClick.AddListener(OnButtonRaiseClick);
    }
    private void OnButtonTrucoClick()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Truco(actorNumber);
    }
    private void OnButtonAcceptClick()
    {
        Accept();
    }
    private void OnButtonRunClick()
    {
        Run();
    }
    private void OnButtonRaiseClick()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Raise(actorNumber);
    }
    public void Truco(int actorNumber)
    {
        GMPhoton.CallTruco(actorNumber);
    }
    private void Accept()
    {
        GMPhoton.CallAccept();
    }
    private void Run()
    {
        GMPhoton.CallRun();
    }
    private void Raise(int actorNumber)
    {
        GMPhoton.CallRaise(actorNumber);
    }
}
