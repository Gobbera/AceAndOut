using UnityEngine;
using Photon.Pun;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Unity.VisualScripting;

public enum GameState { WAITING_PLAYERS, READY_TO_PLAY, STARTING_THE_GAME, DEALING_CARDS, TURN_FROM, COMPARE_CARDS, TURN_WINNER}

public class GameManagerPhoton : MonoBehaviourPunCallbacks
{
    public GameController gameController;
    public GameDataManager GDManager;
    public List<Player> players = new List<Player>();
    public Player[] allPlayers;
    public List<HandCardManager> playerHands = new List<HandCardManager>();
    public int playerCount = 0;
    public static GameManagerPhoton Instance;
    public GameState gameState;
    public Deck deck;
    public Dealer dealer;
    public int cardsPerPlayer = 3;
    public List<CardData> cardsDataOnGame = new List<CardData>();
    public bool hasChanges = false;
    public int currentTurnActor;
    public int lastTurnActor;
    public int turnStep;
    public Player player1;
    public Player player2;
    public bool isFirstRound = true;
    public bool isFirstTurn = true;
    public int firstTurnPlayer;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeState(GameState.WAITING_PLAYERS);
        }
    }
    public void AssignPlayers()
    {
        allPlayers = FindObjectsOfType<Player>();
        foreach (Player p in allPlayers)
        {
            if (p.ActorNumber == 1) player1 = p;
            else if (p.ActorNumber == 2) player2 = p;
        }
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && hasChanges)
        {
            hasChanges = false; // Reseta após processar a alteração
            
            if (playerCount >= 2)
            {
                switch (gameState)
                {
                    case GameState.WAITING_PLAYERS:
                        ChangeState(GameState.READY_TO_PLAY);
                        break;
                    case GameState.READY_TO_PLAY:
                        ChangeState(GameState.STARTING_THE_GAME);
                        break;
                    case GameState.STARTING_THE_GAME:
                        ChangeState(GameState.DEALING_CARDS);
                        break;
                    case GameState.TURN_FROM:
                        ChangeState(GameState.TURN_FROM); //??
                        break;
                }
            }
        }
    }
    private void ChangeState(GameState newState)
    {
        if (PhotonNetwork.IsMasterClient) 
        {
            photonView.RPC("UpdateGameState", RpcTarget.AllBuffered, (int)newState);
        }
    }
    [PunRPC]
    public void UpdateGameState(int newStateInt)
    {
        gameState = (GameState)newStateInt;
        Debug.Log($"[GameManagerPhoton] Novo estado do jogo: {gameState}");

        switch (gameState)
        {
            case GameState.WAITING_PLAYERS:
                gameController.gameLog.changeText("Aguardando Jogadores...");
                break;
            case GameState.READY_TO_PLAY:
                gameController.gameLog.changeText("Adversário encontrado! O jogo vai iniciar em instantes...");
                NotifyChange();
                break;
            case GameState.STARTING_THE_GAME:
                StartCoroutine(StartCountdown());
                break;
            case GameState.DEALING_CARDS:
                gameController.gameLog.changeText("Distribuindo Cartas...");
                AssignPlayers();
                DealCards();
                break;
            case GameState.TURN_FROM:
                string nickname = allPlayers.FirstOrDefault(p => p.ActorNumber == currentTurnActor)?.nickname ?? "Player";
                gameController.gameLog.changeText("Turno de " + nickname + ".");
                break;
            case GameState.COMPARE_CARDS:
                gameController.gameLog.changeText("combate de cartas");
                break;
            case GameState.TURN_WINNER:
                //
                break;
        }
    }
    public void AddPlayerObj(Player player)
    {
        if (player)
        {
            players.Add(player.GetComponent<Player>());
        }
        if (player.handCardManager != null)
        {
            playerHands.Add(player.handCardManager.GetComponent<HandCardManager>());
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InstanceRemotePlayer(newPlayer);
        }
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        /* if (PhotonNetwork.IsMasterClient)
        {
            Instance = this; // O novo MasterClient assume o GameManager
        } */
    }
    public void InstanceRemotePlayer(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && !hasChanges)
        {
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            NotifyChange();
        }
    }
    private IEnumerator StartCountdown()
    {
        yield return gameController.gameLog.StartCountdown();
        ChangeState(GameState.DEALING_CARDS);
        NotifyChange();
    }
    public void NotifyChange()
    {
        if (PhotonNetwork.IsMasterClient && !hasChanges)
        {
            hasChanges = true;
            Debug.Log("[GameManagerPhoton] Notificação de mudança enviada.");
        }
    }
    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int playersCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int totalCardsNeeded = cardsPerPlayer * playersCount;

        if (deck.GetCards().Count < totalCardsNeeded)
        {
            Debug.LogWarning("[GameManagerPhoton] Deck não possui cartas suficientes.");
            return;
        }
        int[] shuffledDeck = deck.ShuffleAndSyncDeck();
        photonView.RPC("ReceiveShuffledDeck", RpcTarget.OthersBuffered, shuffledDeck);
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList)
        {
            photonView.RPC("ReceiveCards", photonPlayer, photonPlayer.ActorNumber);
        }
        DetermineCurrentTurnPlayer();
    }
    [PunRPC]
    public void ReceiveCards(int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            { 
                playerHands[0].AddCard(deck.availableCards[0]);
                playerHands[1].AddHiddenCard();                                 //Temporario
                photonView.RPC("RemoveCard", RpcTarget.AllBuffered);
            }
        }
    }
    [PunRPC]
    public void RemoveCard() 
    {
        deck.RemoveCardData();
    }
    [PunRPC]
    public void ReceiveShuffledDeck(int[] shuffledDeck) 
    {
        deck.ReceiveShuffledDeck(shuffledDeck);
    }
    public void PublishCard(Player player, CardData cardData)
    {
        photonView.RPC("PublishCardRPC", RpcTarget.OthersBuffered, cardData.key);
        photonView.RPC("TurnStepIncrementRPC", RpcTarget.AllBuffered);
        photonView.RPC("PublishThrowerRPC", RpcTarget.AllBuffered, player.ActorNumber);
        photonView.RPC("CompareCardsRPC", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void TurnStepIncrementRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        turnStep++;
    }
    [PunRPC]
    public void PublishCardRPC(int cardKey)
    {
        gameController.gameLog.changeText("Carta Lançada" + cardKey);
        CardData cardData = deck.GetCardById(cardKey);
        players[1].dropZone.UpdateCurrentCard(cardData, players[1], false);
        Destroy(playerHands[1].gameObject.transform.GetChild(0).gameObject);
    }
    [PunRPC]
    public void PublishThrowerRPC(int actorNumber)
    {
        lastTurnActor = actorNumber;
        if (lastTurnActor == player1.ActorNumber)
        {
            player1.isTurn = false;
        }
        else if (lastTurnActor == player2.ActorNumber)
        {
            player2.isTurn = false;
        }
        DetermineCurrentTurnPlayer();
    }
    public void DetermineCurrentTurnPlayer()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (turnStep == 2) return;
        if (isFirstRound)
        {
            var players = PhotonNetwork.PlayerList;
            int randomIndex = Random.Range(0, players.Length);
            int actorNumber = players[randomIndex].ActorNumber;
            photonView.RPC("SetTurnOwner", RpcTarget.AllBuffered, actorNumber); 
            firstTurnPlayer = actorNumber;
            return;
        }
        if (isFirstTurn) 
        {

        }
        if (lastTurnActor == player1.ActorNumber)
        {
            photonView.RPC("SetTurnOwner", RpcTarget.AllBuffered, player2.ActorNumber);
            return;
        }
        else if (lastTurnActor == player2.ActorNumber)
        {
            photonView.RPC("SetTurnOwner", RpcTarget.AllBuffered, player1.ActorNumber);
            return;
        }
    }
    [PunRPC]
    public void SetTurnOwner(int actorNumber)
    {
        if (isFirstRound) { isFirstRound = false; }

        currentTurnActor = actorNumber;
        ChangeState(GameState.TURN_FROM);
        if (currentTurnActor == player1.ActorNumber)
        {
            player1.isTurn = true;
        }
        else if (currentTurnActor == player2.ActorNumber)
        {
            player2.isTurn = true;
        }
    }
    [PunRPC]
    public void CompareCardsRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (turnStep < 2) return;
        if (!player1.dropZone.currentCard || !player2.dropZone.currentCard) return;

        ChangeState(GameState.COMPARE_CARDS);

        Card card1 = player1.dropZone.currentCard.GetComponent<Card>();
        Card card2 = player2.dropZone.currentCard.GetComponent<Card>();

        if (card1.cardValue > card2.cardValue)
        {
            photonView.RPC("DeclareTurnWinner", RpcTarget.AllBuffered, player1.ActorNumber);
        }
        else if (card1.cardValue < card2.cardValue)
        {
            photonView.RPC("DeclareTurnWinner", RpcTarget.AllBuffered, player2.ActorNumber);
        }
        else if (card1.cardValue == card2.cardValue)
        {
            photonView.RPC("DeclareTurnWinner", RpcTarget.AllBuffered, 0);
        }
    }
    [PunRPC]
    public void DeclareTurnWinner(int playerToPlay)
    {
        // Atualiza o estado do jogo
        ChangeState(GameState.TURN_WINNER);

        if (playerToPlay == 0)
        {
            gameController.gameLog.changeText("Empate");
            //GDManager.incrementTurnValue(-1);
            if (isFirstTurn)
            {
                playerToPlay = firstTurnPlayer;
            }
        }
        if (playerToPlay == player1.ActorNumber)
        {
            gameController.gameLog.changeText(player1.nickname + "Venceu o turno" );
            player1.isTurn = true;
            player2.isTurn = false;
            //GDManager.incrementTurnValue(player1.ActorNumber);
            //player1.score++;
            ResetForNextTurn();
            return;
        }
        if (playerToPlay == player2.ActorNumber)
        {
            gameController.gameLog.changeText(player2.nickname + "Venceu o turno" );
            player2.isTurn = true;
            player1.isTurn = false;
            //GDManager.incrementTurnValue(player2.ActorNumber);
            //player2.score++;
            ResetForNextTurn();
            return;
        }
        isFirstTurn = false;
        verifyRoundState();
        //UpdateScoreUI();
    }
    public void ResetForNextTurn()
    {
        turnStep = 0;
        Destroy(player1.dropZone.currentCard);
        Destroy(player2.dropZone.currentCard);
    }
    public void verifyRoundState()
    {}
}
