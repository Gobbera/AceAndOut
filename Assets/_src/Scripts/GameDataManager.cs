using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameDataManager : MonoBehaviourPunCallbacks
{
    public static GameDataManager Instance;
    public int roundNumber = 0;
    public int[] turnWinOrder;
    public int player1Score; 
    public int player2Score; 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void incrementScore(int poits, Player player)
    {
        player1Score += poits;
    }
    public void incrementTurnValue(int playerActN)
    {
        turnWinOrder[0] = playerActN;
    }
    
}
