using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using System.Text;

public class MainMenuManager : MonoBehaviour{

    #region Fields
    static public MainMenuManager S;
    public int songIndex = 0;
    private int _countdownToPlay = 6;
    [SerializeField] private LobbyManager _lobbyManager;
    [SerializeField] private InputField _songNameInputField;
    [SerializeField] private CanvasGroup _messagePanel;
    [SerializeField] private Text _messageHeadline;
    [SerializeField] private Text _messageContent;
    [SerializeField] private Text _errorContent;
    [SerializeField] private Dropdown _songList;
    [SerializeField] private CanvasGroup _countdownPanel;
    [SerializeField] private Text _countdownText;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        ToggleUIElement(_messagePanel, false, true);
        ToggleUIElement(_countdownPanel, false, true);
        
    }

    private void Start()
    {
        DataManager.OnResponseFromServer += DisplayMessage;
        
    }

    private void OnDisable()
    {
        DataManager.OnResponseFromServer -= DisplayMessage;
    }

    #endregion


    #region Public Methods
    /// <summary>
    /// Loads a song from the server and assigns the data to a GameController.songInfo (SongInfo[])
    /// </summary>
    public void LoadSong()
    {
        //songIndex = _songList.value;
        songIndex = UnityEngine.Random.Range(0, 4);
        StartCoroutine(LoadSongCoroutine(songIndex));
    }

    public void DismissMessage()
    {
        ToggleUIElement(_messagePanel, false);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Display notification message
    /// </summary>
    /// <param name="hl">headline</param>
    /// <param name="content"></param>
    /// <param name="error"></param>
    private void DisplayMessage(string hl, string content, string error = "")
    {
        _messageHeadline.text = Reverse(hl);
        _messageContent.text = Reverse(content);
        _errorContent.text = error;
        ToggleUIElement(_messagePanel, true);
    }

    /// <summary>
    /// Used to reverse a string
    /// </summary>
    /// <param name="s">The string as it is now</param>
    /// <returns>The string in reverse</returns>
    private string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Used to fade in/out a UI panel
    /// </summary>
    /// <param name="element">The panel's CanvasGroup component</param>
    /// <param name="show">True: Fade in, False: fade out</param>
    /// <param name="immediate">True: Fade in/out at zero time</param>
    private void ToggleUIElement(CanvasGroup element, bool show, bool immediate = false)
    {
        if (immediate)
        {
            if (show)
            {
                element.gameObject.SetActive(true);
                element.DOFade(1f, 0f);
                element.blocksRaycasts = true;
            }
            else
            {
                element.DOFade(0f, 0f);
                element.blocksRaycasts = false;
                element.gameObject.SetActive(false);
            }
        }
        else
        {
            if (show)
            {
                element.gameObject.SetActive(true);
                element.DOFade(1f, 0.5f);
                element.blocksRaycasts = true;
            }
            else
            {
                element.DOFade(0f, 0.5f);
                element.blocksRaycasts = false;
                element.gameObject.SetActive(false);
            }
        }
    }

    public IEnumerator LoadSongCoroutine(int songIndex)
    {
        //Debug.Log("LoadSongCoroutine");
        DataManager dataManager = FindObjectOfType<DataManager>();
        if (dataManager)
            yield return StartCoroutine(dataManager.GetSongDataListFromServer(songIndex));
        else
            Debug.LogError("DataManager cannot be found");
    }

    IEnumerator LoadGameSceneCoroutine()
    {
        int songIndex = UnityEngine.Random.Range(0, 3);
        //Debug.Log("songIndex: " + songIndex);
        yield return StartCoroutine(LoadSongCoroutine(songIndex));
        _lobbyManager.ServerChangeScene(_lobbyManager.playScene);
    }

    //[ClientRpc]
    public void LoadGameScene()
    {
        StartCoroutine(LoadGameSceneCoroutine());
    }
    #endregion

    #region MatchMaking

    public void OnClickStart()
    {
        StartCoroutine(CreateJoinMatch());
        SetCountDownText(_countdownToPlay);
        ToggleUIElement(_countdownPanel, true);
    }

    public IEnumerator CreateJoinMatch()
    {
        int matchCounter = 0;
        _lobbyManager.StartMatchMaker();
        yield return _lobbyManager.matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleListMatches);
        if (_lobbyManager.matches == null)
            Debug.Log("There are no matches available");
        else
        {
            //Debug.Log("Matches: " + _lobbyManager.matches.Count);
            if (_lobbyManager.matches.Count == 0)
            {
                yield return _lobbyManager.matchMaker.CreateMatch(CreateServerName(6), (uint)_lobbyManager.maxPlayers, true, "", "", "", 0, 0, _lobbyManager.OnMatchCreate);
                _lobbyManager._isMatchmaking = true;
                yield return _lobbyManager.matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleListMatches);
                _lobbyManager.lobbySlots[0].SendReadyToBeginMessage();
            }
            else if (_lobbyManager.matches.Count > 0)
            {
                for (int i = 0; i < _lobbyManager.matches.Count; i++)
                {
                    if (_lobbyManager.matches[i].currentSize == 1)
                    {
                        matchCounter++;
                        JoinMatch(_lobbyManager.matches[i].networkId);
                    }
                }
                if (matchCounter == 0)
                {
                    yield return _lobbyManager.matchMaker.CreateMatch(CreateServerName(6), (uint)_lobbyManager.maxPlayers, true, "", "", "", 0, 0, _lobbyManager.OnMatchCreate);
                    _lobbyManager._isMatchmaking = true;
                    yield return _lobbyManager.matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleListMatches);
                    _lobbyManager.lobbySlots[0].SendReadyToBeginMessage();
                }
            }
            //_lobbyManager.OnLobbyServerPlayersReady();
        }

    }

    public void HandleListMatches(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        _lobbyManager.matches = matches;
        //Debug.Log("success: "+success + " ext info: " + extendedInfo + "match count: " + matches.Count);
    }

    public void JoinMatch(NetworkID networkID)
    {
        Debug.Log("JoinMatch");
        _lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, _lobbyManager.OnMatchJoined);
        _lobbyManager._isMatchmaking = true;
    }

    public string CreateServerName(int length)
    {
        const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder res = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            res.Append(valid[UnityEngine.Random.Range(0, valid.Length)]);
        }
        return res.ToString();
    }

    public void SetCountDownText(int remainingTime)
    {
        _countdownText.text = remainingTime.ToString();
    }
    #endregion
}
