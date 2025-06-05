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
    public void CreateNewDeck()
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
        int rank = cardId / 100;
        int suit = cardId % 10;

        Suit targetSuit;
        switch (suit)
        {
            case 1: targetSuit = Suit.DIAMONDS; break;
            case 2: targetSuit = Suit.SPADES; break;
            case 3: targetSuit = Suit.HEARTS; break;
            case 4: targetSuit = Suit.CLUBS; break;
            default:
                Debug.LogWarning("Suit inválido: " + suit);
                return null;
        }
        Rank targetRank;
        switch (rank)
        {
            case 1: targetRank = Rank.ACE; break;
            case 2: targetRank = Rank.TWO; break;
            case 3: targetRank = Rank.THREE; break;
            case 4: targetRank = Rank.FOUR; break;
            case 5: targetRank = Rank.FIVE; break;
            case 6: targetRank = Rank.SIX; break;
            case 7: targetRank = Rank.SEVEN; break;
            case 11: targetRank = Rank.QUEEN; break;
            case 12: targetRank = Rank.JACK; break;
            case 13: targetRank = Rank.KING; break;
            default:
                Debug.LogWarning("Suit inválido: " + suit);
                return null;
        }
        // Agora faz a busca na lista
        foreach (CardData card in cardDataList)
        {
            if (card.rank == targetRank && card.suit == targetSuit)
            {
                return card;
            }
        }

        Debug.LogWarning($"Carta com rank {rank} e suit {targetSuit} não encontrada.");
        return null;
    }
        
}


