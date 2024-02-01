using System;
using System.Collections.Generic;
using Unity;
using UnityEngine;

public class LevelController
{
    Level currentLevel;
    int movesMade;
    Dictionary<int, int> tilesDestroyed;
    Dictionary<int, int> tilesNeeded;
    public event Action<LevelProgress> OnProgressChange;


    void Notify()
    {
        List<(int, int, int)> tilesProgress = new List<(int, int, int)>();
        foreach (int type in tilesNeeded.Keys)
        {
            tilesProgress.Add((type, tilesDestroyed[type], tilesNeeded[type]));
        }
        OnProgressChange?.Invoke(new LevelProgress
        {
            movesLeft = currentLevel.movesLimit - movesMade,
            tilesProgress = tilesProgress
        });
    }
    public void StartLevel(Level level)
    {
        currentLevel = level;
        movesMade = 0;
        tilesDestroyed = new Dictionary<int, int>();
        tilesNeeded = new Dictionary<int, int>();
        foreach (var objective in currentLevel.destroyTilesObjectives)
        {
            tilesNeeded[objective.tileType] = objective.quantity;
            tilesDestroyed[objective.tileType] = 0;
        }
        Notify();
    }

    public void UpdateObjectives(BoardSequence boardSequence)
    {
        foreach (int type in boardSequence.tilesDestroyedPerType.Keys)
        {
            if (!tilesDestroyed.ContainsKey(type))
            {
                tilesDestroyed[type] = 0;
            }
            tilesDestroyed[type] += boardSequence.tilesDestroyedPerType[type];
        }
        Notify();

    }

    public void MakeMove()
    {
        movesMade += 1;
        Notify();
    }

    public bool IsComplete()
    {
        foreach (var type in tilesNeeded.Keys)
        {
            if (!tilesDestroyed.ContainsKey(type)) { return false; }
            if (tilesNeeded[type] > tilesDestroyed[type]) { return false; }
        }
        return true;
    }
    public bool IsGameOver()
    {
        return movesMade >= currentLevel.movesLimit;
    }
}

public class LevelProgress
{
    public int movesLeft;

    //tileType / current / needed
    public List<(int, int, int)> tilesProgress;
}