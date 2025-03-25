using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private List<CardData> cardDataList; // Lista dos dados das cartas (52 CardData ScriptableObjects)
    [SerializeField] private GameObject cardPrefab; // Prefab da carta que será instanciado
    public List<CardData> availableCards; // Lista para gerenciar cartas disponíveis

    void Start()
    {
        availableCards = new List<CardData>(cardDataList);
        InstantiateDeck();
    }

    private void InstantiateDeck()
    {
        foreach (CardData cardData in availableCards)
        {
            GameObject cardInstance = Instantiate(cardPrefab, transform.position, Quaternion.identity, transform);
            Card cardComponent = cardInstance.GetComponent<Card>();

            if (cardComponent != null)
            {
                cardComponent.cardData = cardData;
                cardComponent.UpdateCardProperties();
            }
        }
    }


    public List<CardData> GetCards()
    {
        return availableCards;
    }

    
    public CardData RemoveCardData()
    {
        if (availableCards.Count > 0)
        {
            CardData cardData = availableCards[0];
            availableCards.RemoveAt(0);
            
            
            Transform cardVisual = transform.Find(cardData.name);
            if (cardVisual != null)
            {
                Destroy(cardVisual.gameObject);
            }
            return cardData;
        }
        Debug.LogWarning("Tentativa de remover carta de um deck vazio.");
        return null;
    }
}
