using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HandCardManager : MonoBehaviour
{
    public GameManagerPhoton GMPhoton;
    [SerializeField] private Player player;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject cardHiddenPrefab;
    private RectTransform handsCardGroup;
    public float maxWidth = 800f;
    public float maxHeight = 200f;
    public List<CardData> cardsInHand = new List<CardData>();
    public void Initialize(Player assignedPlayer)
    {
        player = assignedPlayer;
        handsCardGroup = GetComponent<RectTransform>();
    }
    public void AddCard(CardData card)
    {
        cardsInHand.Add(card);

        GameObject cardInstance = Instantiate(cardPrefab, transform.position, Quaternion.identity);
        Card cardComponent = cardInstance.GetComponent<Card>();

        if (cardComponent != null)
        {
            cardComponent.cardData = card;
            cardComponent.origem = player;
            cardComponent.UpdateCardProperties();
            cardInstance.transform.SetParent(this.transform);
        }
        ReorganizeCards();
    }
    public void AddHiddenCard()
    {
        GameObject cardInstance = Instantiate(cardHiddenPrefab, transform.position, Quaternion.identity);
        CardData card = cardInstance.GetComponent<Card>().cardData;
        cardsInHand.Add(card);
        cardInstance.transform.SetParent(this.transform);
        ReorganizeCards();
    }
    private void ReorganizeCards()
    {
        float cardSpacing = Mathf.Min(maxWidth / cardsInHand.Count, maxWidth / 10);
        float startX = -((cardsInHand.Count - 1) * cardSpacing) / 2f;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            Transform cardTransform = transform.GetChild(i);
            Vector3 newPosition = new Vector3(startX + i * cardSpacing, 0, 0);
            cardTransform.localPosition = newPosition;
        }
    }
}

