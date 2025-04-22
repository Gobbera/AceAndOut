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
        if (!origem || !origem.view.IsMine || !origem.isTurn) return;
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
            Debug.Log("Lançado Carta no DropZone do " + dropZone.dropZoneName);

            OnCardLaunched?.Invoke(origem, cardData);
        }
    }
}
