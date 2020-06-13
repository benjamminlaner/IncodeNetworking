using System.Collections;
using System.Collections.Generic;
using Client;
using UnityEngine;

public class PlayerUpdateSystem : MonoBehaviour
{
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
                LocalPlayer localPlayer = entry.Value.GetComponent<LocalPlayer>();
                localPlayer.transform.position = entry.Value.Pos;
            }
            else
            {
                NetworkPlayer networkPlayer = entry.Value.GetComponent<NetworkPlayer>();
                networkPlayer.transform.position = entry.Value.Pos;
            }
        }
    }
}
