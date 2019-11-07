using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIPlayer : MonoBehaviour {

    
    private float _tapDelayTime = 0.5f;
    private int _score = 0;
    private GameUIManager _gameUIManager;
    [SerializeField]private int _id;
    public int id { get; private set; }

    private void Start()
    {
        enabled = false;
    }

    public void SetID(int newID)
    {
        id = newID;
        Debug.Log("AI id: " + id);
    }

    public void AttemptToHitTile(GameTile gt)
    {
        StartCoroutine(AttemptToHitTileCoroutine(gt));
    }

    private IEnumerator AttemptToHitTileCoroutine(GameTile gt)
    {
        yield return new WaitForSeconds(_tapDelayTime);
        int chance = Random.Range(1, 8);
        if (chance % 5 > 0)
            HitTile(gt);
        else if (chance % 4 == 0)
            MissTile();
    }

    private void HitTile(GameTile gt)
    {
        gt.RpcChangeColor(new Color(0f, 0f, 0f, 0.1f), gt.note.length);
        gt.DisableTouch();
        _score += gt.note.rewardValue;
        if (!_gameUIManager)
            _gameUIManager = GetComponent<GameUIManager>();
        _gameUIManager.SetScore(id, _score);
    }

    private void MissTile()
    {
        float xRandom = Random.Range(-4f, 4f);
        GetComponent<GameTileManager>().CmdMissTile(new Vector3(xRandom, -4.95f, -3.3f), false);
    }
}
