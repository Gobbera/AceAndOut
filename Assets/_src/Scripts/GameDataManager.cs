using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameDataManager : MonoBehaviourPunCallbacks
{
    public GameManagerPhoton GMPhoton;
    public int roundNumber = 0;
    public int[] turnWinOrder = new int[3]; // até 3 turnos por rodada
    private int currentTurnIndex = 0;
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

        if (CheckRoundEnded())
        {
            GMPhoton.isRoundEnded = true;
            int roundWinner = GetRoundWinner();
            if (roundWinner != -1)
            {
                incrementScore(1, roundWinner);
                GMPhoton.DeclareRoundWinner(roundWinner);
                ResetTurnData();
            }
        }
    }

    private bool CheckRoundEnded()
    {
        int p1Wins = 0;
        int p2Wins = 0;

        for (int i = 0; i < currentTurnIndex; i++)
        {
            int winner = turnWinOrder[i];
            if (winner == GMPhoton.player1.ActorNumber)
                p1Wins++;
            else if (winner == GMPhoton.player2.ActorNumber)
                p2Wins++;
        }

        // Alguém venceu dois turnos
        if (p1Wins == 2 || p2Wins == 2)
            return true;

        // Empate + vitória = fim da rodada (só faz sentido verificar no terceiro turno)
        if (currentTurnIndex == 3)
            return true;

        return false;
    }


    private int GetRoundWinner()
    {
        int p1Wins = 0;
        int p2Wins = 0;

        for (int i = 0; i < currentTurnIndex; i++)
        {
            int winner = turnWinOrder[i];
            if (winner == GMPhoton.player1.ActorNumber)
                p1Wins++;
            else if (winner == GMPhoton.player2.ActorNumber)
                p2Wins++;
        }

        // Alguém venceu 2 turnos
        if (p1Wins == 2)
            return GMPhoton.player1.ActorNumber;

        if (p2Wins == 2)
            return GMPhoton.player2.ActorNumber;

        // Dois turnos e um é empate (1 vitória + 1 empate)
        if (currentTurnIndex == 2)
        {
            if (p1Wins == 1 && p2Wins == 0)
                return GMPhoton.player1.ActorNumber;

            if (p2Wins == 1 && p1Wins == 0)
                return GMPhoton.player2.ActorNumber;
        }

        // Três turnos com 1 vitória pra cada e 1 empate
        if (currentTurnIndex == 3 && p1Wins == 1 && p2Wins == 1)
        {
            // Quem venceu o primeiro turno ganha
            for (int i = 0; i < 3; i++)
            {
                if (turnWinOrder[i] == GMPhoton.player1.ActorNumber)
                    return GMPhoton.player1.ActorNumber;

                if (turnWinOrder[i] == GMPhoton.player2.ActorNumber)
                    return GMPhoton.player2.ActorNumber;
            }
        }

        return -1;
    }


}

