using System.Collections;
using System.Collections.Generic;
using Client;
using UnityEngine;

public class PlayerUpdateSystem : MonoBehaviour
{
    [SerializeField]
    private YojimboLocalPlayer localPlayer;
    [SerializeField]
    private YojimboNetworkPlayer networkPlayer;
    public Dictionary<ulong, NetworkPlayerState> playerNetworkDictionary;

    void Awake()
    {

    }

    public void ProcessPlayerUpdates()
    {

    }

    void Update()
    {
        if (playerNetworkDictionary == null || playerNetworkDictionary.Count <= 0) { return; }

        foreach (KeyValuePair<ulong, NetworkPlayerState> entry in playerNetworkDictionary)
        {
            if (entry.Value.IsOwner)
            {
                localPlayer.transform.position = entry.Value.Pos;
            }
            else
            {
                networkPlayer.transform.position = entry.Value.Pos;
            }
        }
    }
}
