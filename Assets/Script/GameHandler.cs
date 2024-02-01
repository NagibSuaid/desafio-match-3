using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] public BoardView boardView;
    [SerializeField] public ScoreboardView scoreboardView;
    [SerializeField] public LevelProgressView levelProgressView;
    [SerializeField] public GameOverScreen gameOverScreen;
    [SerializeField] public RetryScreen retryScreen;
    [SerializeField] public NextLevelScreen nextLevelScreen;
    [SerializeField] public List<Level> levels;
    LevelController levelController;
    int currentLevelIndex;


    private void Awake()
    {
        levelController = new LevelController();
        gameController = new GameController();
    }

    void OnEnable()
    {
        levelController.OnProgressChange += levelProgressView.OnProgressChange;
        boardView.OnTileClick += OnTileClick;
        retryScreen.button.onClick.AddListener(RetryLevel);
        retryScreen.button.onClick.AddListener(() => retryScreen.gameObject.SetActive(false));
        gameOverScreen.button.onClick.AddListener(() => gameOverScreen.gameObject.SetActive(false));
        gameOverScreen.button.onClick.AddListener(RetryLevel);
        nextLevelScreen.button.onClick.AddListener(() => nextLevelScreen.gameObject.SetActive(false));
        nextLevelScreen.button.onClick.AddListener(RetryLevel);
    }
    void OnDisable()
    {
        levelController.OnProgressChange -= levelProgressView.OnProgressChange;
        boardView.OnTileClick -= OnTileClick;
        retryScreen.button.onClick.RemoveAllListeners();
        gameOverScreen.button.onClick.RemoveAllListeners();
        nextLevelScreen.button.onClick.RemoveAllListeners();

    }

    private void Start()
    {
        currentLevelIndex = 0;
        RetryLevel();
        DOTween.SetTweensCapacity(500, 500);
    }

    private int selectedX, selectedY = -1;

    private bool isAnimating;

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) { return; }

        if (selectedX <= -1 || selectedY <= -1)
        {
            selectedX = x;
            selectedY = y;
            boardView.HighlightTile(selectedX, selectedY);
            return;
        }

        if (Mathf.Abs(selectedX - x) + Mathf.Abs(selectedY - y) > 1)
        {
            boardView.ClearHighlightedTiles(selectedX, selectedY);
            selectedX = x;
            selectedY = y;
            boardView.HighlightTile(selectedX, selectedY);
            return;
        }
        if (selectedX == x && selectedY == y)
        {
            boardView.ClearHighlightedTiles(selectedX, selectedY);
            selectedX = -1;
            selectedY = -1;
            return;
        }

        boardView.ClearHighlightedTiles(selectedX, selectedY);
        isAnimating = true;
        boardView.SwapTiles(selectedX, selectedY, x, y).onComplete += () =>
        {
            bool isValid = gameController.IsValidMovement(selectedX, selectedY, x, y);
            if (!isValid)
            {
                boardView.SwapTiles(x, y, selectedX, selectedY)
                .onComplete += () => isAnimating = false;
            }
            else
            {
                List<BoardSequence> swapResult = gameController.SwapTile(selectedX, selectedY, x, y);

                AnimateBoard(swapResult, 0, () => { isAnimating = false; CheckForEndOfLevel(); });
            }
            selectedX = -1;
            selectedY = -1;
        };
    }

    private void CheckForEndOfLevel()
    {
        if (levelController.IsComplete())
        {
            GoToNextLevel();
        }
        else if (levelController.IsGameOver())
        {
            OpenRetryScreen();
        }
    }

    private void OpenRetryScreen()
    {
        retryScreen.gameObject.SetActive(true);
    }
    private void RetryLevel()
    {
        levelController.StartLevel(levels[currentLevelIndex]);
        boardView.DestroyBoard();
        List<List<Tile>> board = gameController.StartGame(levels[currentLevelIndex].boardWidth, levels[currentLevelIndex].boardHeight, levels[currentLevelIndex].colorsUsed);
        boardView.CreateBoard(board);
    }

    private void GoToNextLevel()
    {
        currentLevelIndex += 1;
        if (currentLevelIndex >= levels.Count)
        {
            OpenGameOverScreen();
        }
        else
        {
            OpenNextLevelScreen();
        }
    }

    private void OpenNextLevelScreen()
    {
        nextLevelScreen.gameObject.SetActive(true);
    }
    private void OpenGameOverScreen()
    {
        currentLevelIndex = 0;
        gameOverScreen.gameObject.SetActive(true);
    }

    private void AnimateBoard(List<BoardSequence> boardSequences, int i, Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        BoardSequence boardSequence = boardSequences[i];
        sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition));
        sequence.Append(boardView.ExplodeTiles(boardSequence.explosions));
        sequence.Join(scoreboardView.UpdateScore(boardSequence.pointsGained, boardSequence.newPointTotal));
        sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
        sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

        levelController.UpdateObjectives(boardSequence);
        if (i == 0)
        {
            levelController.MakeMove();
        }

        i++;
        if (i < boardSequences.Count)
        {
            sequence.onComplete += () => AnimateBoard(boardSequences, i, onComplete);
        }
        else
        {
            sequence.onComplete += () => onComplete();
        }
    }
}
