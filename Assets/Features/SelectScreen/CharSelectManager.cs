using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CharSelectManager : NetworkBehaviour
{
    public static CharSelectManager Instance { get; private set;}

    private Dictionary<ulong, bool> playerReadyDictionary;

    void Awake()
    {
        Instance = this;
        playerReadyDictionary = new();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientReady = true;
        foreach(ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                allClientReady = false;
                break;
            }
        }

        if(allClientReady)
        {
            Loader.LoadNetwork(Loader.SceneType.Game);
        }
    }
}
