using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private int turn; // Definido pela quantidade de jogadores
    [SerializeField] private int round; // Definido pela quantidade de cartas na mão de cada jogador
    //[SerializeField] private PlayerId currentPlayerToPlay;

    public void Initialize(int assignedRound, List<Player> assignedPlayers)
    {
        turn = assignedRound * assignedPlayers.Count;
        round = assignedRound;
        //currentPlayerToPlay = assignedPlayers[] as PlayerId;
    } 

    //public void SetCurrentPlayerToPlay (PlayerId playerId) { currentPlayerToPlay = playerId; }
}
