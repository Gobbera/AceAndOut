using System.Collections.Generic;
using UnityEngine;

public class Dealer : MonoBehaviour
{
    public void DealCards(int cardsPerPlayer, Deck deck, List<HandCardManager> playerHands)
    {
        int totalCardsNeeded = cardsPerPlayer * playerHands.Count;
    
        if (deck.GetCards().Count < totalCardsNeeded)
        {
            Debug.LogWarning("Deck não possui cartas suficientes para distribuir a quantidade desejada.");
            return;
        }
        foreach (HandCardManager hand in playerHands)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                CardData cardToDeal = deck.RemoveCardData();
                if (cardToDeal != null)
                {
                    hand.AddCard(cardToDeal);
                }
                else
                {
                    Debug.LogWarning("Deck está vazio! Não há mais cartas para distribuir.");
                    return;
                }
            }
        }
    }

}
