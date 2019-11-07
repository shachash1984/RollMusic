using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Networking;

public class GameTile : NetworkBehaviour {

    public Note note;
    public bool touched = false;
    public bool host;
    [SyncVar] public Color currentColor = Color.black;
    public GameTileManager gameTileManager;

    private void Update()
    {
        if (!gameTileManager)
            gameTileManager = FindObjectOfType<GameTileManager>();
    }

    [ClientRpc]
    public void RpcChangeColor(Color c, float tileLength)
    {
        currentColor = c;
        GetComponent<SpriteRenderer>().DOColor(currentColor, 0.3f*tileLength/NotePanel.TILE_SIZE).SetEase(Ease.Linear);
    }

    public void DisableTouch()
    {
        touched = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            gameTileManager.RemoveTile(this);
            Destroy(gameObject);
        }
    }

}
