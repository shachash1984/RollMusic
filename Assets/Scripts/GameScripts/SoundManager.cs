using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SoundManager : NetworkBehaviour {

    #region Fields
    static public SoundManager S;
    public AudioSource[] audioSources;
    public AudioClip[] clips;
    public Dictionary<string, AudioClip> clipMap = new Dictionary<string, AudioClip>();
    public NetworkIdentity networkIdentity;
    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        Init();
        
    }

    private void OnEnable()
    {
        if (!isLocalPlayer)
            return;
        GameTileManager.OnMiss += RpcPlayMissedSound;
    }

    private void OnDisable()
    {
        if (!isLocalPlayer)
            return;
        GameTileManager.OnMiss -= RpcPlayMissedSound;
    }

    

    #endregion

    #region Methods
    private void Init()
    {
        audioSources = GetComponents<AudioSource>();
        networkIdentity = GetComponent<NetworkIdentity>();
        foreach (AudioClip ac in clips)
        {
            clipMap.Add(ac.name, ac);
        }
    }

    [Client]
    public void PlaySound(string soundName)
    {
        if (!isLocalPlayer)
            return;
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying || audioSources[i].time > 1.5f)
            {
                audioSources[i].clip = clipMap[soundName];
                audioSources[i].Play();
                break;
            }
        }
    }

    [ClientRpc]
    public void RpcPlaySound(string soundName)
    {
        PlaySound(soundName);
    }

    [ClientRpc]
    public void RpcPlayMissedSound()
    {
        for (int i = 0; i < 3 - 1; i++)
        {
            audioSources[i].clip = clips[i];
            audioSources[i].Play();
        }
    }
    #endregion

}
