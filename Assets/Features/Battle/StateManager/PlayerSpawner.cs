using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Unit unitPrefab;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadComplete;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadComplete;
    }

    private void OnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientIds, List<ulong> clientTimedOut)
    {
        foreach (var clientId in clientIds)
        {
            Unit instance = Instantiate(unitPrefab);
            instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }
}
