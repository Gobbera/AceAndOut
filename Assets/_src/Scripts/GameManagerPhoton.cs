using UnityEngine;
using Photon.Pun;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Unity.VisualScripting;
using System;

public enum GameState { WAITING_PLAYERS, READY_TO_PLAY, STARTING_THE_GAME, DEALING_CARDS, TURN_FROM, TURN_WINNER}

public class GameManagerPhoton : MonoBehaviourPunCallbacks
{
    public GameController gameController;
    public UIController uIController;
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
    public CardData trumpCard;
    public Player player1;
    public Player player2;
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
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartGame();
    }
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ChangeState(GameState.WAITING_PLAYERS);
    }
    public void AssignPlayers()
    {
        allPlayers = FindObjectsOfType<Player>();
        foreach (Player p in allPlayers)
        {
            if (p.ActorNumber == 1)
            {
                player1 = p;
                gameController.scoreP1NickTag.changeText(player1.nickname);
            }
            else if (p.ActorNumber == 2)
            {
                player2 = p;
                gameController.scoreP2NickTag.changeText(player2.nickname);
            }
        }
        player1.canPlay = true;
        player2.canPlay = true;
    }
    private void ChangeState(GameState newState)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_UpdateGameState", RpcTarget.AllBuffered, (int)newState);
    }
    [PunRPC]
    public void RPC_UpdateGameState(int newStateInt)
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
                ExecuteAfterDelay(1f, () =>
                {
                    NotifyChange();
                });
                break;
            case GameState.STARTING_THE_GAME:
                StartCoroutine(StartCountdown());
                break;
            case GameState.DEALING_CARDS:
                if (allPlayers.Count() < 2)
                {
                    AssignPlayers();
                }
                ExecuteAfterDelay(1f, () =>
                {
                    photonView.RPC("RPC_UpdateGameLogMessage", RpcTarget.AllBuffered, 1);
                    DealCards();
                    //SetTrumpCards(); //metodo futuro
                });
                break;
            case GameState.TURN_FROM:
                Player player = GetPlayerActed(currentTurnActor);
                player.PlayerTurn();
                gameController.gameLog.changeText("Turno de" + " " + player.nickname);
                //if (!player.isRaiseCaller)
                //{   
                    uIController.trucoButton.EnableInteractableButton();
                //}
                break;
            case GameState.TURN_WINNER:
                //
                break;
        }
    }
    [PunRPC]
    public void RPC_UpdateGameLogMessage(int msg)
    {
        switch (msg)
        {
            case 1:
                gameController.gameLog.changeText("Distribuindo Cartas...");
                break;
            default:
                break;
        }
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && hasChanges)
        {
            hasChanges = false;
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
                }
            }
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        InstanceRemotePlayer(newPlayer);
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
    public void NotifyChange()
    {
        if (PhotonNetwork.IsMasterClient && !hasChanges) hasChanges = true;
    }
    private IEnumerator StartCountdown()
    {
        yield return gameController.gameLog.StartCountdown();
        ChangeState(GameState.DEALING_CARDS);
        NotifyChange();
    }
    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int playersCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int totalCardsNeeded = cardsPerPlayer * playersCount;

        if (deck.GetCards().Count < totalCardsNeeded)
        {
            Debug.LogWarning("Deck não possui cartas suficientes.");
            return;
        }
        int[] shuffledDeck = deck.ShuffleAndSyncDeck();
        photonView.RPC("RPC_ReceiveShuffledDeck", RpcTarget.AllBuffered, shuffledDeck);
        photonView.RPC("RPC_TurnUpTrumpCard", RpcTarget.AllBuffered);
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList)
        {
            photonView.RPC("RPC_ReceiveCards", photonPlayer, photonPlayer.ActorNumber);
        }
        DetermineCurrentTurnPlayer();
    }
    [PunRPC]
    public void RPC_TurnUpTrumpCard()
    {
        trumpCard = deck.availableCards[0];
        dealer.TurnUpACard(trumpCard);
        photonView.RPC("RPC_RemoveCard", RpcTarget.AllBuffered);
    }
    public void SetTrumpCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_SetTrumpCards", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_SetTrumpCards()
    {
        // 1. Descobrir o rank da carta virada
        Rank turnedUpRank = trumpCard.rank;

        // 2. Descobrir o próximo rank na ordem do truco
        int index = GDManager.trucoOrder.IndexOf(turnedUpRank);
        int trumpIndex = (index + 1) % GDManager.trucoOrder.Count;
        Rank trumpRank = GDManager.trucoOrder[trumpIndex];

        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        // 3. Para todas as cartas do baralho
        foreach (GameObject cardObj in cards)
        {
            Card card = cardObj.GetComponent<Card>();
            if (card.rank == trumpRank)
            {
                switch (card.suit)
                {
                    case Suit.DIAMONDS: card.UpdateToTrumpCard(100); break;
                    case Suit.SPADES: card.UpdateToTrumpCard(1000); break;
                    case Suit.HEARTS: card.UpdateToTrumpCard(10000); break;
                    case Suit.CLUBS: card.UpdateToTrumpCard(100000); break;
                }
            }
        }
    }
    [PunRPC]
    public void RPC_ReceiveCards(int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                playerHands[0].AddCard(deck.availableCards[0]);
                playerHands[1].AddHiddenCard();                                 //Temporario
                photonView.RPC("RPC_RemoveCard", RpcTarget.AllBuffered);
            }
        }
    }
    [PunRPC]
    public void RPC_RemoveCard()
    {
        deck.RemoveCardData();
    }
    [PunRPC]
    public void RPC_ReceiveShuffledDeck(int[] shuffledDeck)
    {
        deck.ReceiveShuffledDeck(shuffledDeck);
    }
    public void PublishCard(Player player, CardData cardData)
    {
        photonView.RPC("RPC_PublishCard", RpcTarget.OthersBuffered, cardData.key);
        photonView.RPC("RPC_TurnStepIncrement", RpcTarget.AllBuffered);
        photonView.RPC("RPC_PublishThrower", RpcTarget.AllBuffered, player.ActorNumber);
        photonView.RPC("RPC_CompareCardsRPC", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void RPC_TurnStepIncrement()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GDManager.turnStep++;
    }
    [PunRPC]
    public void RPC_PublishCard(int cardKey)
    {
        gameController.gameLog.changeText("Carta Lançada" + cardKey);
        CardData cardData = deck.GetCardById(cardKey);
        Debug.Log(cardData);
        players[1].dropZone.UpdateCurrentCard(cardData, players[1], false);
        Destroy(playerHands[1].gameObject.transform.GetChild(0).gameObject);
    }
    [PunRPC]
    public void RPC_PublishThrower(int actorNumber)
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
        if (GDManager.isRoundEnded) return;
        if (GDManager.turnStep == 2) return;
        if (GDManager.isFirstRound)
        {
            var players = PhotonNetwork.PlayerList;
            int randomIndex = UnityEngine.Random.Range(0, players.Length);
            int actorNumber = players[randomIndex].ActorNumber;
            currentTurnActor = actorNumber;
            photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, currentTurnActor);
            firstTurnPlayer = actorNumber;
            return;
        }
        if (GDManager.isFirstTurn && !GDManager.isFirstRound && GDManager.turnStep == 0)
        {
            Player dealer = players.Find(p => p.isDealer);
            Player firstPlayer = players.Find(p => !p.isDealer);
            if (firstPlayer.ActorNumber == player1.ActorNumber)
            {
                currentTurnActor = player2.ActorNumber;
                photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, currentTurnActor);
                return;
            }
            else if (firstPlayer.ActorNumber == player2.ActorNumber)
            {
                currentTurnActor = player1.ActorNumber;
                photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, currentTurnActor);
                return;
            }
        }
        if (lastTurnActor == player1.ActorNumber)
        {
            currentTurnActor = player2.ActorNumber;
            photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, currentTurnActor);
            return;
        }
        else if (lastTurnActor == player2.ActorNumber)
        {
            currentTurnActor = player1.ActorNumber;
            photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, currentTurnActor);
            return;
        }
    }
    [PunRPC]
    public void RPC_SetTurnOwner(int actorNumber)
    {
        if (GDManager.isRoundEnded) return;
        currentTurnActor = actorNumber;
        ChangeState(GameState.TURN_FROM);
        UpdateTrucarButton();
        if (GDManager.isFirstRound)
        {
            if (currentTurnActor == player1.ActorNumber)
            {
                player1.isDealer = true;
                player1.isFoot = true;
            }
            else if (currentTurnActor == player2.ActorNumber)
            {
                player2.isDealer = true;
                player1.isFoot = true;
            }
            GDManager.isFirstRound = false;
        }
        if (currentTurnActor == player1.ActorNumber)
        {
            if (GDManager.turnStep == 0) { player1.isFoot = true; }
            player1.isTurn = true;
        }
        else if (currentTurnActor == player2.ActorNumber)
        {
            if (GDManager.turnStep == 0) { player2.isFoot = true; }
            player2.isTurn = true;
        }
    }
    [PunRPC]
    public void RPC_CompareCardsRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (GDManager.turnStep < 2) return;
        if (!player1.dropZone.currentCard || !player2.dropZone.currentCard) return;

        Rank turnedUpRank = trumpCard.rank;

        int index = GDManager.trucoOrder.IndexOf(turnedUpRank);
        int trumpIndex = (index + 1) % GDManager.trucoOrder.Count;
        Rank trumpRank = GDManager.trucoOrder[trumpIndex];

        Card card1 = player1.dropZone.currentCard.GetComponent<Card>();
        Card card2 = player2.dropZone.currentCard.GetComponent<Card>();

        if (card1.rank == trumpRank)
        {
            switch (card1.suit)
            {
                case Suit.DIAMONDS: card1.UpdateToTrumpCard(100); break;
                case Suit.SPADES: card1.UpdateToTrumpCard(1000); break;
                case Suit.HEARTS: card1.UpdateToTrumpCard(10000); break;
                case Suit.CLUBS: card1.UpdateToTrumpCard(100000); break;
            }
        }
        if (card2.rank == trumpRank)
        {
            switch (card2.suit)
            {
                case Suit.DIAMONDS: card2.UpdateToTrumpCard(100); break;
                case Suit.SPADES: card2.UpdateToTrumpCard(1000); break;
                case Suit.HEARTS: card2.UpdateToTrumpCard(10000); break;
                case Suit.CLUBS: card2.UpdateToTrumpCard(100000); break;
            }
        }

        if (card1.cardCombatValue > card2.cardCombatValue)
        {
            photonView.RPC("RPC_DeclareTurnWinner", RpcTarget.AllBuffered, player1.ActorNumber);
        }
        else if (card1.cardCombatValue < card2.cardCombatValue)
        {
            photonView.RPC("RPC_DeclareTurnWinner", RpcTarget.AllBuffered, player2.ActorNumber);
        }
        else if (card1.cardCombatValue == card2.cardCombatValue)
        {
            photonView.RPC("RPC_DeclareTurnWinner", RpcTarget.AllBuffered, 0);
        }

    }
    [PunRPC]
    public void RPC_DeclareTurnWinner(int playerToPlay)
    {
        if (GDManager.isRoundEnded) return;
        uIController.trucoButton.DisableInteractableButton();
        ChangeState(GameState.TURN_WINNER);
        player1.isTurn = false;
        player2.isTurn = false;
        if (playerToPlay == 0)
        {
            GDManager.isDraw = true;
            gameController.gameLog.changeText("Empate");
            GDManager.incrementTurnValue(-1);
            Player footPlayer = players.Find(p => p.isFoot);
            playerToPlay = footPlayer.ActorNumber;
        }
        if (playerToPlay == player1.ActorNumber)
        {
            player1.isFoot = true;
            player2.isFoot = false;
            if (!GDManager.isDraw)
            {
                gameController.gameLog.changeText(player1.nickname + " Venceu o turno");
                GDManager.incrementTurnValue(player1.ActorNumber);
            }
        }
        if (playerToPlay == player2.ActorNumber)
        {
            player1.isFoot = false;
            player2.isFoot = true;
            if (!GDManager.isDraw)
            {
                gameController.gameLog.changeText(player2.nickname + " Venceu o turno");
                GDManager.incrementTurnValue(player2.ActorNumber);
            }
        }
        if (GDManager.isFirstTurn) { GDManager.isFirstTurn = false; }
        if (CheckRoundEnded())
        {
            GDManager.isRoundEnded = true;
            int roundWinner = GetRoundWinner();
            if (roundWinner != -1)
            {
                GDManager.incrementScore(roundWinner);
                DeclareRoundWinner(roundWinner);
                GDManager.ResetTurnData();
            }
            return;
        }
        InitNextTurn(playerToPlay);
    }
    private bool CheckRoundEnded()
    {
        int p1Wins = 0;
        int p2Wins = 0;
        int draw = 0;

        for (int i = 0; i < GDManager.currentTurnIndex; i++)
        {
            int winner = GDManager.turnWinOrder[i];
            if (winner == player1.ActorNumber)
                p1Wins++;
            else if (winner == player2.ActorNumber)
                p2Wins++;
            else if (winner == -1)
                draw++;
        }

        // Alguém venceu dois turnos
        if (p1Wins == 2 || p2Wins == 2)
            return true;

        if (p1Wins == 1 && draw >= 1 || p2Wins == 1 && draw >= 1)
            return true;
        // Empate + vitória = fim da rodada
        if (GDManager.currentTurnIndex == 3)
            return true;

        return false;
    }
    private int GetRoundWinner()
    {
        int p1Wins = 0;
        int p2Wins = 0;

        for (int i = 0; i < GDManager.currentTurnIndex; i++)
        {
            int winner = GDManager.turnWinOrder[i];
            if (winner == player1.ActorNumber)
                p1Wins++;
            else if (winner == player2.ActorNumber)
                p2Wins++;
        }

        // Alguém venceu 2 turnos
        if (p1Wins == 2)
            return player1.ActorNumber;

        if (p2Wins == 2)
            return player2.ActorNumber;

        // Dois turnos e um é empate (1 vitória + 1 empate)
        if (GDManager.currentTurnIndex == 2)
        {
            if (p1Wins == 1 && p2Wins == 0)
                return player1.ActorNumber;

            if (p2Wins == 1 && p1Wins == 0)
                return player2.ActorNumber;
        }

        // Três turnos com 1 vitória pra cada e 1 empate
        if (GDManager.currentTurnIndex == 3 && p1Wins == 1 && p2Wins == 1)
        {
            // Quem venceu o primeiro turno ganha
            for (int i = 0; i < 3; i++)
            {
                if (GDManager.turnWinOrder[i] == player1.ActorNumber) //adicionar isFirstRoundWinner
                    return player1.ActorNumber;

                if (GDManager.turnWinOrder[i] == player2.ActorNumber)
                    return player2.ActorNumber;
            }
        }
        return -1;
    }
    [PunRPC]
    public void RPC_ResetForNextTurn()
    {
        GDManager.isDraw = false;
        GDManager.turnStep = 0;
        Destroy(player1.dropZone.currentCard);
        Destroy(player2.dropZone.currentCard);
    }
    public void InitNextTurn(int playerToPlay)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (GDManager.isRoundEnded) return;
        ExecuteAfterDelay(2f, () =>
        {
            photonView.RPC("RPC_ResetForNextTurn", RpcTarget.AllBuffered);
            photonView.RPC("RPC_InitNextTurn", RpcTarget.AllBuffered, playerToPlay);
        });
    }
    [PunRPC]
    public void RPC_InitNextTurn(int playerToPlay)
    {
        if (GDManager.isRoundEnded) return;
        uIController.trucoButton.EnableInteractableButton();
        if (playerToPlay == player1.ActorNumber)
        {
            player1.isTurn = true;
            player2.isTurn = false;
            gameController.gameLog.changeText("Turno de " + player1.nickname);
        }
        if (playerToPlay == player2.ActorNumber)
        {
            player1.isTurn = false;
            player2.isTurn = true;
            gameController.gameLog.changeText("Turno de " + player2.nickname);
        }
    }
    public void DeclareRoundWinner(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_DeclareRoundWinner", RpcTarget.AllBuffered, actorNumber);
    }
    [PunRPC]
    public void RPC_DeclareRoundWinner(int actorNumber)
    {
        if (actorNumber == player1.ActorNumber)
        {
            gameController.gameLog.changeText(player1.nickname + " Venceu a rodada!");
            gameController.scoreP1.changeText(GDManager.player1Score.ToString());
            // aplicar outros efeitos, animar, etc.
        }
        else if (actorNumber == player2.ActorNumber)
        {
            gameController.gameLog.changeText(player2.nickname + " Venceu a rodada!");
            gameController.scoreP2.changeText(GDManager.player2Score.ToString());
        }
        GDManager.roundNumber++;
        if (!PhotonNetwork.IsMasterClient) return;
        ResetForNextRound();
    }
    public void ResetForNextRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ExecuteAfterDelay(2f, () =>
        {
            photonView.RPC("RPC_ResetForNextRound", RpcTarget.AllBuffered);
        });
    }

    [PunRPC]
    public void RPC_ResetForNextRound()
    {
        if (GDManager.isFirstRound) { GDManager.isFirstRound = false; }
        GDManager.isRoundEnded = false;
        GDManager.roundValue = 1;
        GDManager.turnStep = 0;
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
        player1.handCardManager.cardsInHand.Clear();
        player2.handCardManager.cardsInHand.Clear();
        player1.canPlay = true;
        player2.canPlay = true;
        player1.isRaiseCaller = false;
        player2.isRaiseCaller = false;
        uIController.trucoButton.changeText("Truco");
        uIController.DisableWaitingTxt();
        deck.CreateNewDeck();
        foreach (Player player in players)
        {
            player.isDealer = !player.isDealer;
        }
        StartNewRound();
    }
    public void StartNewRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (CheckGameEnded())
        {
            int gameWinner = GetGameWinner();
            ExecuteAfterDelay(1f, () =>
            {
                photonView.RPC("RPC_AnuncePlayerWinner", RpcTarget.AllBuffered, gameWinner);
            });
            return;
        }
        ExecuteAfterDelay(1f, () =>
        {
            photonView.RPC("RPC_StartNewRound", RpcTarget.AllBuffered);
        });
    }
    public int GetGameWinner()
    {
        if (GDManager.player1Score >= 12) return player1.ActorNumber;
        if (GDManager.player2Score >= 12) return player2.ActorNumber;
        else return -1;
    }
    public bool CheckGameEnded()
    {
        if (GDManager.player1Score >= 12 || GDManager.player2Score >= 12) return true;
        return false;
    }
    [PunRPC]
    public void RPC_AnuncePlayerWinner(int playerWinner)
    {
        string nickname = allPlayers.FirstOrDefault(p => p.ActorNumber == playerWinner)?.nickname ?? "Player";
        gameController.gameLog.changeText("Jogador" + " " + nickname + " " + "Venceu o Jogo!");
    }
    [PunRPC]
    public void RPC_StartNewRound()
    {
        gameController.gameLog.changeText("Iniciando uma nova rodada");
        ChangeState(GameState.DEALING_CARDS);
    }
    public void ExecuteAfterDelay(float delayInSeconds, Action callback)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        double startTime = PhotonNetwork.Time;
        photonView.RPC("RPC_StartDelay", RpcTarget.All, startTime, delayInSeconds);
        delayedCallback = callback;
    }

    private Action delayedCallback;

    [PunRPC]
    private void RPC_StartDelay(double startTime, float delayInSeconds)
    {
        StartCoroutine(SynchronizedDelay(startTime, delayInSeconds));
    }

    private IEnumerator SynchronizedDelay(double startTime, float delayInSeconds)
    {
        double timeToWait = (startTime + delayInSeconds) - PhotonNetwork.Time;

        if (timeToWait > 0)
            yield return new WaitForSeconds((float)timeToWait);

        delayedCallback?.Invoke();
    }
    public void UpdateTrucarButton()
    {
        Player player = GetPlayerActed(currentTurnActor);
        if (PhotonNetwork.LocalPlayer.ActorNumber == currentTurnActor)
        {
            //if (!GDManager.canRaise) return;
            if (player.isRaiseCaller) return;
            uIController.trucoButton.EnableButton();
        }
        else
        {
            uIController.trucoButton.DisableButton();
        }
    }
    [PunRPC]
    public void PlayersCanPlay(bool canPlay)
    {
        if (canPlay)
        {
            player1.canPlay = true;
            player2.canPlay = true;
        }
        else
        {
            player1.canPlay = false;
            player2.canPlay = false;
        }
    }
    public Player GetPlayerActed(int actorNumber)
    {
        Player player = allPlayers.FirstOrDefault(p => p.ActorNumber == actorNumber);
        return player;
    }
    public void CallTruco(int actorNumber)
    {
        //if (!GDManager.canRaise) return;
        uIController.trucoButton.DisableButton();
        uIController.EnableWaitingTxt();
        photonView.RPC("TrucoState", RpcTarget.OthersBuffered);
        photonView.RPC("PlayersCanPlay", RpcTarget.AllBuffered, false);
        photonView.RPC("ChangeRoundValueState", RpcTarget.AllBuffered);
        photonView.RPC("RaiseCaller", RpcTarget.AllBuffered, actorNumber);
    }
    [PunRPC]
    public void RaiseCaller(int actorNumber)
    {
        SoundManager.Instance.TrucoNotification();

        Player player = GetPlayerActed(actorNumber);

        if (player.isRaiseCaller)
        {
            // Se já é RaiseCaller, desmarca (toggle off)
            player.isRaiseCaller = false;
        }
        else
        {
            // Se não é RaiseCaller, desmarca todos e marca este (toggle on)
            foreach (var p in allPlayers)
            {
                p.isRaiseCaller = false;
            }
            player.isRaiseCaller = true;
        }
    }
    [PunRPC]
    public void TrucoState()
    {
        uIController.acceptButton.EnableButton();
        uIController.runButton.EnableButton();
        uIController.raiseButton.EnableButton();
    }
    public void CallAccept()
    {
        photonView.RPC("AcceptState", RpcTarget.AllBuffered);
        photonView.RPC("PlayersCanPlay", RpcTarget.AllBuffered, true);
    }
    [PunRPC]
    public void AcceptState()
    {
        GDManager.isRaised = true;
        uIController.acceptButton.DisableButton();
        uIController.runButton.DisableButton();
        uIController.raiseButton.DisableButton();
        uIController.DisableWaitingTxt();
    }
    public void CallRun()
    {
        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        int otherActorNumber = PhotonNetwork.PlayerList
            .FirstOrDefault(p => p.ActorNumber != myActorNumber)?.ActorNumber ?? -1;

        if (otherActorNumber != -1)
        {
            photonView.RPC("RunState", RpcTarget.AllBuffered, otherActorNumber);
        }
        else
        {
            Debug.LogWarning("Outro jogador não encontrado!");
        }
    }
    [PunRPC]
    public void RunState(int actorNumber)
    {
        uIController.DisableWaitingTxt();
        uIController.acceptButton.DisableButton();
        uIController.runButton.DisableButton();
        uIController.raiseButton.DisableButton();
        if (GDManager.roundValue == 3) GDManager.roundValue = 1;
        if (GDManager.roundValue == 6) GDManager.roundValue = 3;
        if (GDManager.roundValue == 9) GDManager.roundValue = 6;
        if (GDManager.roundValue == 12) GDManager.roundValue = 9;
        GDManager.incrementScore(actorNumber);
        DeclareRoundWinner(actorNumber);
    }
    public void CallRaise(int actorNumber)
    {
        uIController.EnableWaitingTxt();
        GDManager.canRaise = false;
        uIController.acceptButton.DisableButton();
        uIController.runButton.DisableButton();
        uIController.raiseButton.DisableButton();
        photonView.RPC("RaiseState", RpcTarget.Others);
        photonView.RPC("ChangeRoundValueState", RpcTarget.AllBuffered);
        photonView.RPC("RaiseCaller", RpcTarget.AllBuffered, actorNumber);
    }
    [PunRPC]
    public void RaiseState()
    {
        SoundManager.Instance.TrucoNotification();
        //if (GDManager.canRaise)
        //{
            uIController.raiseButton.EnableButton();
            uIController.acceptButton.EnableButton();
            uIController.runButton.EnableButton();
        //}
        if (GDManager.roundValue >= 9)
        {
            uIController.raiseButton.DisableButton();
        }
    }
    [PunRPC]
    public void ChangeRoundValueState()
    {
        if (GDManager.roundValue == 9)
        {
            GDManager.roundValue = 12;
            //GDManager.canRaise = false;
        }
        if (GDManager.roundValue == 6)
        {
            GDManager.roundValue = 9;
            uIController.raiseButton.changeText("12");
            uIController.trucoButton.changeText("12");
        }
        if (GDManager.roundValue == 3)
        {
            uIController.raiseButton.changeText("9");
            uIController.trucoButton.changeText("9");
            GDManager.roundValue = 6;
        }
        if (GDManager.roundValue == 1)
        {
            uIController.trucoButton.changeText("6");
            uIController.raiseButton.changeText("6");
            GDManager.roundValue = 3;
        }
    }
}

/*
    BUG: Alternacia de Turno esta errado

    ?Bug, advesraio consegue trucar de alguma maneira ai mesmo se nao e o turno dele
    se estivar trucado, durante o jogo o jogador deve conseguir dar raise no valor e outro nao pode mais trucar
    
    melhorias graficas e adicionar som
    
    e tela de menu com tutorial
    
    consertar a hitbox
    
    Fazer um compnente de raise


    falta possibilidade de fugir em qualquer momento da partida
    falta mao de onze
    falta a possibilidade de jogar a carta escondida
*/
