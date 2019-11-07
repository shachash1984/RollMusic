using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {

    public Camera playerCamera;
    public NetworkIdentity networkIdentity;
    public AIPlayer AIPlayer;
    public int id;

    public void Start()
    {
        if (!playerCamera)
            playerCamera = GetComponentInChildren<Camera>();
        networkIdentity = GetComponent<NetworkIdentity>();
        if (isLocalPlayer)
            SetID();
        playerCamera.enabled = isLocalPlayer;
        playerCamera.GetComponent<AudioListener>().enabled = isLocalPlayer;
        GetComponent<SoundManager>().enabled = isLocalPlayer;
        Debug.Log("numPlayers: " + NetworkManager.singleton.numPlayers);
        if (!AIPlayer)
            AIPlayer = GetComponent<AIPlayer>();
        AIPlayer.enabled = isLocalPlayer;
        if (isLocalPlayer)
            InitAI();
    }

    public void InitAI()
    {
        AIPlayer.enabled = NetworkManager.singleton.numPlayers == 1 ? true : false;
        if (!AIPlayer.enabled)
            return;
        if (id == 2)
            AIPlayer.SetID(1);
        else
            AIPlayer.SetID(2);
    }
    
    public void SetID()
    {
        if (transform.position.z > 0)
            id = 2;
        else
            id = 1;
        Debug.Log("playerID: " + id);
    }
}
