using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Loading,
    GamePlaying,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance {get; private set;}

    public event Action onStartLoading;
    public event Action onStartGamePlaying;
    public event Action onStartGameOver;

    private int totalPlayers;
    private int connectedPlayers = 0;

    void Awake()
    {
        Instance = this;
    }


    public void CallStartLoading()
    {
        onStartLoading?.Invoke();
    }

    public void CallStartGame()
    {
        onStartGamePlaying?.Invoke();
    }
}
