using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameUIManager : NetworkBehaviour {

    #region Fields
    [SerializeField] private Image player1Image;
    [SerializeField] private Image player2Image;
    [SerializeField] private Text player1ScoreText;
    [SerializeField] private Text player2ScoreText;
    private GameTileManager _gameTileManager;
    #endregion

    #region Monobehaviour Callbacks
    public void Start()
    {
        Init();
    }

    public override void OnStartLocalPlayer()
    {
        SetScore(1, 0);
        SetScore(2, 0);
    }
    #endregion

    #region Methods


    public void Init()
    {
        SetScore(1, 0);
        SetScore(2, 0);
    }

    
    [ClientRpc]
    public void RpcAddPlayerScore(int newScore)
    {
        SetScore(1, newScore);
    }

    [ClientRpc]
    public void RpcAddOpponentScore(int newScore)
    {
        SetScore(2, newScore);
    }

    public void SetScore(int index, int newScore)
    {
        if (index == 1)
            player1ScoreText.text = newScore.ToString();
        else
            player2ScoreText.text = newScore.ToString();
    }

    #endregion

}
