using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MissedGameTile : NetworkBehaviour {

    GameTileManager gameTileManager;

    private void Update()
    {
        if (!gameTileManager)
            gameTileManager = FindObjectOfType<GameTileManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            gameTileManager.RemoveMissedTile(gameObject);
            Destroy(gameObject);
        }
    }
}
