using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class GameNetworkingManager : NetworkManager
{
    private List<Transform> _curSpawnPoints = new List<Transform>();

    public static Action<GameObject> OnPlayerCreated;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);

        OnPlayerCreated?.Invoke(player);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            _curSpawnPoints = new List<Transform>(startPositions);
        }

        base.OnServerSceneChanged(sceneName);
    }

    public override Transform GetStartPosition()
    {
        startPositions.RemoveAll(t => t == null);

        if (startPositions.Count == 0)
            return null;

        if (playerSpawnMethod == PlayerSpawnMethod.Random)
        {
            int ind = UnityEngine.Random.Range(0, _curSpawnPoints.Count);

            Transform curPos = _curSpawnPoints[ind];

            _curSpawnPoints.RemoveAt(ind);
            return curPos;
        }
        else
        {
            Transform startPosition = startPositions[startPositionIndex];
            startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
            return startPosition;
        }
    }
}