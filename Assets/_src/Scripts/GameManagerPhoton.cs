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
    public bool isDraw = false;
    public bool isRoundEnded = false;
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
            if (p.ActorNumber == 1) player1 = p;
            else if (p.ActorNumber == 2) player2 = p;
        }
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
                gameController.gameLog.changeText("Distribuindo Cartas...");
                if (allPlayers.Count() < 2)
                {
                    AssignPlayers();
                }
                ExecuteAfterDelay(1f, () =>
                {
                    DealCards();
                });
                break;
            case GameState.TURN_FROM:
                string nickname = allPlayers.FirstOrDefault(p => p.ActorNumber == currentTurnActor)?.nickname ?? "Player";
                gameController.gameLog.changeText("Turno de" + " " + nickname);
                break;
            case GameState.TURN_WINNER:
                //
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
        photonView.RPC("RPC_ReceiveShuffledDeck", RpcTarget.OthersBuffered, shuffledDeck);
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList)
        {
            photonView.RPC("RPC_ReceiveCards", photonPlayer, photonPlayer.ActorNumber);
        }
        DetermineCurrentTurnPlayer();
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
        turnStep++;
    }
    [PunRPC]
    public void RPC_PublishCard(int cardKey)
    {
        gameController.gameLog.changeText("Carta Lançada" + cardKey);
        CardData cardData = deck.GetCardById(cardKey);
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
        if (isRoundEnded) return;
        if (turnStep == 2) return;
        if (isFirstRound)
        {
            var players = PhotonNetwork.PlayerList;
            int randomIndex = UnityEngine.Random.Range(0, players.Length);
            int actorNumber = players[randomIndex].ActorNumber;
            photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, actorNumber);
            firstTurnPlayer = actorNumber;
            return;
        }
        if (isFirstTurn)
        {
            //Aqui sera ao contrario do jogador firstRound para simular a troca de dealer
        }
        if (lastTurnActor == player1.ActorNumber)
        {
            photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, player2.ActorNumber);
            return;
        }
        else if (lastTurnActor == player2.ActorNumber)
        {
            photonView.RPC("RPC_SetTurnOwner", RpcTarget.AllBuffered, player1.ActorNumber);
            return;
        }
    }
    [PunRPC]
    public void RPC_SetTurnOwner(int actorNumber)
    {
        if (isRoundEnded) return;
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
    public void RPC_CompareCardsRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (turnStep < 2) return;
        if (!player1.dropZone.currentCard || !player2.dropZone.currentCard) return;

        Card card1 = player1.dropZone.currentCard.GetComponent<Card>();
        Card card2 = player2.dropZone.currentCard.GetComponent<Card>();

        if (card1.cardValue > card2.cardValue)
        {
            photonView.RPC("RPC_DeclareTurnWinner", RpcTarget.AllBuffered, player1.ActorNumber);
        }
        else if (card1.cardValue < card2.cardValue)
        {
            photonView.RPC("RPC_DeclareTurnWinner", RpcTarget.AllBuffered, player2.ActorNumber);
        }
        else if (card1.cardValue == card2.cardValue)
        {
            photonView.RPC("RPC_DeclareTurnWinner", RpcTarget.AllBuffered, 0);
        }

    }
    [PunRPC]
    public void RPC_DeclareTurnWinner(int playerToPlay)
    {
        if (isRoundEnded) return;
        ChangeState(GameState.TURN_WINNER);
        player1.isTurn = false;
        player2.isTurn = false;
        if (playerToPlay == 0)
        {
            isDraw = true;
            gameController.gameLog.changeText("Empate");
            GDManager.incrementTurnValue(-1);
            if (isFirstTurn)
            {
                playerToPlay = firstTurnPlayer;
                isFirstTurn = false;
            }
        }
        if (playerToPlay == player1.ActorNumber)
        {
            if (!isDraw) gameController.gameLog.changeText(player1.nickname + " Venceu o turno");
            GDManager.incrementTurnValue(player1.ActorNumber);
        }
        if (playerToPlay == player2.ActorNumber)
        {
            if (!isDraw) gameController.gameLog.changeText(player2.nickname + " Venceu o turno");
            GDManager.incrementTurnValue(player2.ActorNumber);
        }
        if (CheckRoundEnded())
        {
            isRoundEnded = true;
            int roundWinner = GetRoundWinner();
            if (roundWinner != -1)
            {
                GDManager.incrementScore(1, roundWinner);
                DeclareRoundWinner(roundWinner);
                GDManager.ResetTurnData();
            }
            return;
        }
        InitNextTurn(playerToPlay);
        //UpdateScoreUI();
    }
    private bool CheckRoundEnded()
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

        // Alguém venceu dois turnos
        if (p1Wins == 2 || p2Wins == 2)
            return true;

        // Empate + vitória = fim da rodada (só faz sentido verificar no terceiro turno)
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
                if (GDManager.turnWinOrder[i] == player1.ActorNumber)
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
        isDraw = false;
        turnStep = 0;
        Destroy(player1.dropZone.currentCard);
        Destroy(player2.dropZone.currentCard);
    }
    public void InitNextTurn(int playerToPlay)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isRoundEnded) return;
        ExecuteAfterDelay(2f, () =>
        {
            photonView.RPC("RPC_ResetForNextTurn", RpcTarget.AllBuffered);
            photonView.RPC("RPC_InitNextTurn", RpcTarget.AllBuffered, playerToPlay);
        });
    }
    [PunRPC]
    public void RPC_InitNextTurn(int playerToPlay)
    {
        if (isRoundEnded) return;
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
            // aplicar outros efeitos, animar, etc.
        }
        else if (actorNumber == player2.ActorNumber)
        {
            gameController.gameLog.changeText(player2.nickname + " Venceu a rodada!");
        }

        GDManager.roundNumber++;

        StartNewRound();
    }
    [PunRPC]
    public void RPC_ResetForNextRound()
    {
        isRoundEnded = false;
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            Destroy(card);
        }   
        //Criar um novo Deck
    }
    public void StartNewRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ExecuteAfterDelay(2f, () =>
        {
            photonView.RPC("RPC_ResetForNextRound", RpcTarget.AllBuffered);
            photonView.RPC("RPC_StartNewRound", RpcTarget.AllBuffered);
        });
    }
    [PunRPC]
    public void RPC_StartNewRound()
    {
        gameController.gameLog.changeText("Iniciando uma nova rodada");        
        //ChangeState(GameState.DEALING_CARDS);
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
}

/*
    começou o jogo
    (é o primeiro round?)
        Sim -> escolher o primeiro jogador
        Não -> quem foi o ultimo delaer()? -> proximo jogador sera quem o jogador a frente do dealer 
*/
