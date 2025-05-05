using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Deck : MonoBehaviourPun
{
    [SerializeField] private List<CardData> cardDataList; // Todos os CardData do jogo
    public List<CardData> availableCards; // Lista das cartas embaralhadas

    void Start()
    {
        availableCards = new List<CardData>(cardDataList);
    }
    // Somente o Master embaralha e envia a ordem para os outros
    public int[] ShuffleAndSyncDeck()
    {
        int[] shuffledIndexes = new int[availableCards.Count];
        for (int i = 0; i < shuffledIndexes.Length; i++) shuffledIndexes[i] = i;

        for (int i = 0; i < shuffledIndexes.Length; i++)
        {
            int rand = Random.Range(i, shuffledIndexes.Length);
            (shuffledIndexes[i], shuffledIndexes[rand]) = (shuffledIndexes[rand], shuffledIndexes[i]);
        }

        // Reorganiza localmente
        List<CardData> shuffledCards = new List<CardData>();
        foreach (int index in shuffledIndexes)
        {
            shuffledCards.Add(cardDataList[index]);
        }
        availableCards = shuffledCards;

        // Envia ordem para os outros jogadores
        return shuffledIndexes;
    }
    public void ReceiveShuffledDeck(int[] shuffledIndexes)
    {
        availableCards = new List<CardData>();
        foreach (int index in shuffledIndexes)
        {
            availableCards.Add(cardDataList[index]);
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
    public CardData GetCardById(int cardId)
    {
        int rank = cardId / 100; // Ex: 1204 → 12 (Dama)
        int suit = cardId % 10;  // Ex: 1204 → 4 (Clubs)

        // SuitMap: 1 = Diamonds, 2 = Spades, 3 = Hearts, 4 = Clubs
        // Agora como a ordem é [rank][suit], usamos o seguinte índice:
        int suitOffset;
        switch (suit)
        {
            case 1: suitOffset = 1; break; // Diamonds
            case 2: suitOffset = 3; break; // Spades
            case 3: suitOffset = 2; break; // Hearts
            case 4: suitOffset = 0; break; // Clubs
            case 5: suitOffset = 5; break; // Hidden
            case 6: suitOffset = 6; break; // Joker
            default: return null;
        }

        int index = ((rank - 1) * 4) + suitOffset;

        if (index < 0 || index >= cardDataList.Count)
            return null;

        return cardDataList[index];
    }

}
