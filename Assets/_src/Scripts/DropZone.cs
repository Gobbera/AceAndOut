using UnityEngine;
public class DropZone : MonoBehaviour
{
    public GameManagerPhoton GMPhoton;
    [SerializeField] private GameObject cardPrefab;
    public string dropZoneName;
    public GameObject currentCard;
    public void UpdateCurrentCard(CardData cardData, Player origem)
    {
        currentCard = Instantiate(cardPrefab, transform.position, Quaternion.identity, transform);
        Card cardComponent = currentCard.GetComponent<Card>();
        if (cardComponent != null)
        {
            cardComponent.cardData = cardData;
            cardComponent.origem = origem;
            cardComponent.UpdateCardProperties();
            Debug.Log("Carta No DropZone do Jogador" + dropZoneName);
        }
        cardComponent.cardInDropZone = true;
        GMPhoton.PublishCard(origem, cardData);
    }
}
