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
}
