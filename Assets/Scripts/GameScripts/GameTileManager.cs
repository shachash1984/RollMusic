using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using DG.Tweening;
using UnityEngine.UI;

public class GameTileManager : NetworkBehaviour {

    #region Fields
    //static public GameTileManager S;
    public NetworkIdentity networkIdentity;
    public Player player;
    public Song song;
    public GameManager gameManager;
    private SoundManager _soundManager;
    private GameUIManager _gameUIManager;
    private AIPlayer _AIPlayer;
    [SyncVar] public int playerID;
    public int playerScore = 0;
    public int opponentScore = 0;
    [SerializeField] private GameObject _botDestroyTrigger;
    [SerializeField] private Vector3[] _spawnPositions;
    [SerializeField] private GameObject _gameTilePrefab;
    [SerializeField] private GameObject _longGameTilePrefab;
    [SerializeField] private GameObject _missGameTilePrefab;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _missedTileSpeed = 15f;
    [SerializeField] [Range(0, 1)] private float _speedDamper = 0.65f;
    private Vector3 _hostSpawnRotation = new Vector3(90, 0, 0);
    private Vector3 _clientSpawnRotation = new Vector3(90, 180, 0);
    [SerializeField] private List<GameTile> _activeHostGameTiles = new List<GameTile>();
    [SerializeField] private List<GameTile> _activeClientGameTiles = new List<GameTile>();
    private List<GameObject> _missedHostGameTiles = new List<GameObject>();
    private List<GameObject> _missedClientGameTiles = new List<GameObject>();
    //private Camera _mainCam;
    private Color _hitColor = new Color(0f, 0f, 0f, 0.1f);
    private int _gameTileCounter = 0;
    private const int TILE_LAYER = 512;
    private const int TILE_RUNWAY_LAYER = 256;
    private const int MISSED_TILE_LAYER = 4096;
    private const int UI_LAYER = 32;
    public const float TILE_SIZE = 5f;
    private float timeCounter = 0;
    
    #endregion


    #region Events and Delegates
    public delegate void OnHit(int playerIndex, int score);
    [SyncEvent] public event OnHit EventOnHit;
    public static event Action OnMiss;
    public static event Action<GameTile> OnGameTileIgnored;
    #endregion

    #region MonoBehaviour Callbacks

    public override void OnStartLocalPlayer()
    {
        Init();
    }

    /*private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        if (networkIdentity.isServer)
        {
            //timeCounter += Time.fixedDeltaTime;
            timeCounter += Time.deltaTime;
            if (_gameTileCounter < song.notes.Count && timeCounter >= song.notes[_gameTileCounter].tileTime)
                SpawnTile();
            MoveTiles();
        }
    }*/
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (networkIdentity.isServer)
        {
            //timeCounter += Time.fixedDeltaTime;
            timeCounter += Time.deltaTime;
            if (_gameTileCounter < song.notes.Count && timeCounter >= song.notes[_gameTileCounter].tileTime)
                SpawnTile();
            MoveTiles();
        }

#if !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Vector3 touchPos = Input.touches[0].position;
            int touchPhase = (int)Input.touches[0].phase;
            Ray ray = Camera.main.ScreenPointToRay(touchPos);
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;
            CmdHandleTouch(o, d, touchPhase);
        }
#endif

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;
            CmdHandleMouse(o, d);
        }
    }
#endregion

#region Methods
    private void Init()
    {
        if (!isLocalPlayer)
            return;
        gameManager = GameManager.S;
        player = GetComponent<Player>();
        _gameUIManager = GetComponent<GameUIManager>();
        _soundManager = GetComponent<SoundManager>();
        networkIdentity = GetComponent<NetworkIdentity>();
        _gameTileCounter = 0;
        _activeHostGameTiles.Clear();
        _activeClientGameTiles.Clear();
        song = gameManager.GetSong();
        gameManager.gameTileManager = this;
        ToggleBotDestroyTrigger(false);
    }

    [Command]
    public void CmdMissTile(Vector3 point, bool isHost)
    {
        //aligning tile x POS
        if (point.x < -3)
            point.x = -4.25f;
        else if (point.x < 0)
            point.x = -1.5f;
        else if (point.x < 3)
            point.x = 1.5f;
        else
            point.x = 4.5f;

        //checking to see if host or client
        _soundManager.RpcPlayMissedSound();
        GameObject newGameTile = null;
        if (point.z > 0)
            newGameTile = Instantiate(_missGameTilePrefab, point, Quaternion.Euler(_clientSpawnRotation));
        else
            newGameTile = Instantiate(_missGameTilePrefab, point, Quaternion.Euler(_hostSpawnRotation));
        if (!isHost)
        {
            point.y *= -1;
            _missedClientGameTiles.Add(newGameTile);
        }
        else
        {
            _missedHostGameTiles.Add(newGameTile);
        }
            
        NetworkServer.Spawn(newGameTile);
        Sequence seq = DOTween.Sequence();
        seq.Append(newGameTile.GetComponent<SpriteRenderer>().DOFade(0f, 0.25f));
        seq.Append(newGameTile.GetComponent<SpriteRenderer>().DOFade(1f, 0.25f));
        seq.SetLoops(10);
        seq.Play().SetEase(Ease.Linear).OnComplete(() =>
        {
            if (isHost)
                _missedHostGameTiles.Remove(newGameTile);
            else
                _missedClientGameTiles.Remove(newGameTile);
            Destroy(newGameTile);
        });
    }

    [Command]
    private void CmdHandleTouch(Vector3 origin, Vector3 direction, int touchPhase)
    {
        int score = 0;
        if (touchPhase == 0)
        {
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 150f, _layerMask))
            {
                int hitLayerValue = ((1 << hit.collider.gameObject.layer) & _layerMask);
                if (hitLayerValue == TILE_LAYER)
                {
                    GameTile gt = hit.collider.gameObject.GetComponent<GameTile>();
                    if (!gt.touched)
                    {
                        gt.RpcChangeColor(_hitColor, gt.note.length);
                        gt.DisableTouch();
                        score = gt.note.rewardValue;
                        playerScore += score;
                        string sound = gt.note.soundName;
                        if (!_soundManager)
                            _soundManager = GetComponent<SoundManager>();
                        _soundManager.RpcPlaySound(sound);
                        
                    }
                }
                else if (hitLayerValue == UI_LAYER)
                {
                    return;
                }
                else if (hitLayerValue == TILE_RUNWAY_LAYER)
                {
                    CmdMissTile(hit.point, networkIdentity.isServer);
                    RpcReducePlayerScore();
                }
            }
        }
        if (!_gameUIManager)
            _gameUIManager = GetComponent<GameUIManager>();
        if (!player)
            player = GetComponent<Player>();
        _gameUIManager.SetScore(player.id, playerScore + score);
        
    }

    [Command]
    private void CmdHandleMouse(Vector3 origin, Vector3 direction)
    {
        int score = 0;
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 150f, _layerMask))
        {
            int hitLayerValue = ((1 << hit.collider.gameObject.layer) & _layerMask);
            if (hitLayerValue == TILE_LAYER)
            {
                GameTile gt = hit.collider.gameObject.GetComponent<GameTile>();
                if (!gt.touched)
                {
                    gt.RpcChangeColor(_hitColor, gt.note.length);
                    gt.DisableTouch();
                    score = gt.note.rewardValue;
                    playerScore += score;
                    string sound = gt.note.soundName;
                    if (!_soundManager)
                        _soundManager = GetComponent<SoundManager>();
                    _soundManager.RpcPlaySound(sound);
                    
                }
            }
            else if (hitLayerValue == UI_LAYER)
            {
                return;
            }
            else if (hitLayerValue == TILE_RUNWAY_LAYER)
            {
                CmdMissTile(hit.point, networkIdentity.isServer);
                RpcReducePlayerScore();
            }
        }
        if (!_gameUIManager)
            _gameUIManager = GetComponent<GameUIManager>();
        if (!player)
            player = GetComponent<Player>();
        _gameUIManager.SetScore(player.id, playerScore + score);
    }

    private void MoveTiles()
    {
        for (int i = 0; i < _activeHostGameTiles.Count; i++)
        {
            if (_activeHostGameTiles[i])
                _activeHostGameTiles[i].transform.Translate(Vector3.up * Time.deltaTime * _activeHostGameTiles[i].note.speed);
        }
        for (int i = 0; i < _missedHostGameTiles.Count; i++)
        {
            if (_missedHostGameTiles[i])
                _missedHostGameTiles[i].transform.Translate(Vector3.down * Time.deltaTime * _missedTileSpeed);
        }
        for (int i = 0; i < _activeClientGameTiles.Count; i++)
        {
            if (_activeClientGameTiles[i])
                _activeClientGameTiles[i].transform.Translate(Vector3.up * Time.deltaTime * _activeClientGameTiles[i].note.speed);
        }
        for (int i = 0; i < _missedClientGameTiles.Count; i++)
        {
            if (_missedClientGameTiles[i])
                _missedClientGameTiles[i].transform.Translate(Vector3.down * Time.deltaTime * _missedTileSpeed);
        }
    }

    public void GameTileIgnored(GameTile gt)
    {
        if (gt.host)
            _activeHostGameTiles.Remove(gt);
        else
            _activeClientGameTiles.Remove(gt);
        Destroy(gt.gameObject);
        if (OnGameTileIgnored != null)
            OnGameTileIgnored(gt);
    }

    public bool ShiftColumnIndex()
    {
        foreach (Note n in song.notes)
        {
            if (n.column < 0)
                return true;
        }
        return false;
    }

    public void SpawnTile()
    {
        int offset = 0;
        if (ShiftColumnIndex())
            offset++;
        GameObject prefab = _gameTilePrefab;
        

        //initializing host tiles
        GameObject hostGameTile = Instantiate(prefab, _spawnPositions[song.notes[_gameTileCounter].column + offset], Quaternion.Euler(_hostSpawnRotation), Camera.main.transform);
        GameTile hgt = hostGameTile.GetComponent<GameTile>();
        hgt.note = song.notes[_gameTileCounter];
        hgt.host = true;
        _activeHostGameTiles.Add(hgt);
        Vector3 newScale = hostGameTile.transform.localScale;
        newScale.y = hgt.note.length;
        hgt.transform.localScale = newScale;
        hgt.gameTileManager = this;

        //initializing client tiles
        GameObject clientGameTile = Instantiate(prefab, _spawnPositions[_spawnPositions.Length - 1 - (song.notes[_gameTileCounter].column + offset)], Quaternion.Euler(_clientSpawnRotation), Camera.main.transform);
        GameTile cgt = clientGameTile.GetComponent<GameTile>();
        cgt.note = song.notes[_gameTileCounter];
        cgt.host = false;
        _activeClientGameTiles.Add(cgt);
        newScale = clientGameTile.transform.localScale;
        newScale.y = cgt.note.length;
        cgt.transform.localScale = newScale;
        cgt.gameTileManager = this;

        //incrementing counter
        _gameTileCounter++;
        
        //spawnning tiles
        CmdSpawnTile(hostGameTile);
        CmdSpawnTile(clientGameTile);

        //Activate the AI only if there is only 1 player
        if(NetworkManager.singleton.numPlayers == 1)
        {
            if (!_AIPlayer)
                _AIPlayer = GetComponent<AIPlayer>();
            if (_AIPlayer.id == 1)
                HandleAI(cgt);
            else
                HandleAI(hgt);
        }
    }

    public void HandleAI(GameTile gt)
    {
        if (!_AIPlayer)
            _AIPlayer = GetComponent<AIPlayer>();
        if (!_AIPlayer.enabled)
        {
            _AIPlayer.enabled = true;
            ToggleBotDestroyTrigger(true);
        }
        else if (_AIPlayer.enabled)
        {
            _AIPlayer.AttemptToHitTile(gt);
        }
    }

    public void ToggleBotDestroyTrigger(bool on)
    {
        _botDestroyTrigger.SetActive(on);
    }

    public void RemoveTile(GameTile t)
    {
        if (_activeHostGameTiles.Contains(t))
            _activeHostGameTiles.Remove(t);
        else if (_activeClientGameTiles.Contains(t))
            _activeClientGameTiles.Remove(t);
    }

    public void RemoveMissedTile(GameObject missedTile)
    {
        if (_missedHostGameTiles.Contains(missedTile))
            _missedHostGameTiles.Remove(missedTile);
        else if (_missedClientGameTiles.Contains(missedTile))
            _missedClientGameTiles.Remove(missedTile);
    }
    
#endregion

#region Network Commands
    
    [Command]
    public void CmdOnHitTile(int playerIndex, int score)
    {
        if (EventOnHit != null)
            EventOnHit(playerIndex, score);
    }


    [ClientRpc]
    public void RpcReducePlayerScore()
    {
        playerScore--;
        if (_gameUIManager)
            _gameUIManager = GetComponent<GameUIManager>();
        _gameUIManager.SetScore(1, playerScore);
    }

    [Command]
    private void CmdSpawnTile(GameObject gameTile)
    {
        NetworkServer.Spawn(gameTile);
    }

    #endregion
}
