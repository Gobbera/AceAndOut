using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Card card;

    void Awake()
    {
        card = GetComponent<Card>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!card.origem || !card.origem.view.IsMine || card.cardInDropZone) return;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!card.origem || !card.origem.view.IsMine || card.cardInDropZone) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!card.origem || !card.origem.view.IsMine || card.cardInDropZone) return;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
