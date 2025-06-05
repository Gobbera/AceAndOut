using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Photon.Pun;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Player origem;
    public CardData cardData;
    public Suit suit;
    public Rank rank;
    public int cardValue;
    public int cardCombatValue;
    public bool hasPicked;
    public bool isHidden;
    private Image imageComponent;
    private bool isMouseOver = false;
    public bool cardInDropZone = false;
    // Evento para notificar que a carta foi lançada
    public static event Action<Player, CardData> OnCardLaunched;
    void Start()
    {
        imageComponent = GetComponent<Image>();
        
        UpdateCardProperties();
    }
    public void UpdateCardProperties()
    {
        if (cardData != null && imageComponent != null)
        {
            imageComponent.sprite = cardData.cardSprite;
        }
        suit = cardData.suit;
        rank = cardData.rank;
        cardValue = cardData.cardValue;
        name = cardData.name;
        cardCombatValue = cardValue;
    }
    public void UpdateToTrumpCard(int newValue)
    {
        cardCombatValue = newValue;
        Debug.Log(cardData.cardName + "Agora vale" + cardCombatValue);
    }
    // EVENTS
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!origem || !origem.view.IsMine || !origem.isTurn || !origem.canPlay) return;
        if (isMouseOver && eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Carta a ser lançada: " + rank + " " + suit);
            LaunchCard();
        }
    }
    void LaunchCard()
    {
        if (!cardInDropZone)
        {
            DropZone dropZone = origem.dropZone;
            dropZone.UpdateCurrentCard(cardData, origem);
            Destroy(gameObject);
            OnCardLaunched?.Invoke(origem, cardData);
            origem.handCardManager.RemoveCard(cardData);
        }
    }
}
