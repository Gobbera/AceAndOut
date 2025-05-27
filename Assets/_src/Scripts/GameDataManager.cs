using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameDataManager : MonoBehaviourPunCallbacks
{
    public GameManagerPhoton GMPhoton;
    public int roundNumber = 0;
    public int[] turnWinOrder = new int[3]; // até 3 turnos por rodada
    public int currentTurnIndex = 0;
    public int player1Score;
    public int player2Score;

    private void Awake()
    {
        ResetTurnData();
    }
    public void ResetTurnData()
    {
        turnWinOrder = new int[3] { -2, -2, -2 }; // -2 = ainda não jogado
        currentTurnIndex = 0;
    }

    public void incrementScore(int points, int actorNumber)
    {
        if (actorNumber == GMPhoton.player1.ActorNumber)
        {
            player1Score += points;
        }
        else if (actorNumber == GMPhoton.player2.ActorNumber)
        {
            player2Score += points;
        }
    }

    public void incrementTurnValue(int actorNumber)
    {
        if (currentTurnIndex >= 3) return;

        turnWinOrder[currentTurnIndex] = actorNumber;
        currentTurnIndex++;
    }
}

