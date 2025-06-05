using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviourPunCallbacks
{
    public GameManagerPhoton GMPhoton;
    public int roundNumber = 0;
    public int[] turnWinOrder = new int[3]; // até 3 turnos por rodada
    public int currentTurnIndex = 0;
    public int player1Score;
    public int player2Score;
    public int turnStep;
    public bool isFirstRound = true;
    public bool isFirstTurn = true;
    public bool isDraw = false;
    public bool isRoundEnded = false;
    public bool isRaised = false;
    public int roundValue = 1;
    public List<Rank> trucoOrder = new List<Rank>()
    {
        Rank.FOUR,
        Rank.FIVE,
        Rank.SIX,
        Rank.SEVEN,
        Rank.QUEEN,
        Rank.JACK,
        Rank.KING,
        Rank.ACE,
        Rank.TWO,
        Rank.THREE
    };

    private void Awake()
    {
        ResetTurnData();
    }
    public void ResetTurnData()
    {
        turnWinOrder = new int[3] { -2, -2, -2 }; // -2 = ainda não jogado
        currentTurnIndex = 0;
    }

    public void incrementScore(int actorNumber)
    {
        if (actorNumber == GMPhoton.player1.ActorNumber)
        {
            player1Score += roundValue;
        }
        else if (actorNumber == GMPhoton.player2.ActorNumber)
        {
            player2Score += roundValue;
        }
    }

    public void incrementTurnValue(int actorNumber)
    {
        if (currentTurnIndex >= 3) return;

        turnWinOrder[currentTurnIndex] = actorNumber;
        currentTurnIndex++;
    }
}

