using System.Collections.Generic;
using UnityEngine;

public class Dealer : MonoBehaviour
{   
    public GameObject TurnedCard;
    public GameObject cardPrefab;
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
    public void TurnUpACard(CardData cardData)
    {
        GameObject Card = Instantiate(cardPrefab, TurnedCard.transform.position, Quaternion.identity, transform);
        Card cardComponent = Card.GetComponent<Card>();
        cardComponent.cardData = cardData;
        cardComponent.UpdateCardProperties();
        Card.transform.SetParent(TurnedCard.transform);
    }
}
