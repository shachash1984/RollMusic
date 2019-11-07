using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

public enum GameState { Standby = 0, Play, Pause}

public class GameManager : NetworkBehaviour {

    #region Fields and Properties
    static public GameManager S;
    [SerializeField] private GameState _gameState;
    public GameState gameState
    {
        get { return _gameState; }
        private set { _gameState = value; }
    }
    public int hostScore = 0;
    public int clientScore = 0;
    public Song song;
    public GameTileManager gameTileManager;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        DataManager.OnReceivedSongDataFromServer += UpdateSongInfo;
    }

    private void OnDisable()
    {
        DataManager.OnReceivedSongDataFromServer -= UpdateSongInfo;
    }
    #endregion

    #region Methods
    public Song GetSong()
    {
        return this.song;
    }

    private void UpdateSongInfo(Song s)
    {
        this.song = s;
    }

    
    #endregion

    

}
