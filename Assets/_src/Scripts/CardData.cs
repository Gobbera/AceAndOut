using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public int key;
    public string cardName;
    public Rank rank;
    public Suit suit;
    public int cardValue;
    public Sprite cardSprite;
}
