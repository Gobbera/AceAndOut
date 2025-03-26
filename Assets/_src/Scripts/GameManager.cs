using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum Step { INIT_GAME, END_GAME, INIT_ROUND, END_ROUND, INIT_TURN, END_TURN  }

public class GameManager : MonoBehaviour
{
    /* [SerializeField] private GameController gameController;
    [SerializeField] private Round round;
    private List<Player> players;
    private void OnEnable() { Card.OnCardLaunched += OnCardLaunchedHandler; }
    private void OnDisable() { Card.OnCardLaunched -= OnCardLaunchedHandler; }
    private void OnCardLaunchedHandler(Player player, CardData cardData)
    { 
        GameStep(Step.END_TURN, player);
    }
    public void StartGame(List<Player> players)
    {
        this.players = players;
        round.Initialize(3, players);
        GameStep(Step.INIT_GAME, null);
    }
    

    private Player FindInitialPlayer()
    {
        int startPlayerId = 1;
        int startIndex = players.FindIndex(p => p.playerIndex == startPlayerId);

        if (startIndex != -1) return players[startIndex];
        else
        {
            Debug.LogError($"Nenhum jogador encontrado com o playerIndex {startPlayerId}");
            return null;
        }
    }
    private void DetermineNextPlayer(Player currentPlayer)
    {
        int currentIndex = players.FindIndex(p => p == currentPlayer);
        
        if (currentIndex == -1)
        {
            Debug.LogError("Jogador atual não encontrado na lista de jogadores!");
            return;
        }
        int nextIndex = (currentIndex + 1) % players.Count;

        //round.SetCurrentPlayerToPlay(players[nextIndex].playerId);
        GameStep(Step.INIT_TURN, players[nextIndex]);
    } */
} 