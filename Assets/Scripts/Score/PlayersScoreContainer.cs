using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class PlayersScoreContainer : NetworkBehaviour
{
    [SerializeField] private int _hitsNeededToWin = 3;

    private Dictionary<GameObject, int> _players = new Dictionary<GameObject, int>();

    public string ScoreText => _scoreText;
    [SyncVar(hook = nameof(HandleScoreTextUpdated))]
    private string _scoreText;

    public string WinnerName => _winnerName;
    [SyncVar(hook = nameof(HandleWinnderNameUpdated))]
    private string _winnerName;

    public static Action OnScoreTextUpdated;
    public static Action OnGameEnded;

    public override void OnStartServer() => GameNetworkingManager.OnPlayerCreated += AddPlayer;

    [ServerCallback]
    private void OnDestroy() => GameNetworkingManager.OnPlayerCreated -= AddPlayer;

    [Server]
    public void AddPlayer(GameObject player)
    {
        if (_players.Keys.Contains(player))
            return;

        _players.Add(player, 0);

        player.GetComponent<PlayerScore>().Setup(this);

        UpdateText();
    }

    [Server]
    public void RemovePlayer(GameObject player)
    {
        if (!_players.Keys.Contains(player))
            return;

        _players.Remove(player);

        UpdateText();
    }

    [Server]
    public void ChangeScoreForPlayer(GameObject player, int score)
    {
        if (!_players.Keys.Contains(player))
            return;

        _players[player] = score;

        UpdateText();

        CheckForWin(score);
    }

    [Server]
    private void CheckForWin(int score)
    {
        if (_hitsNeededToWin <= score)
        {
            if (_players.FirstOrDefault(x => x.Value == score).Key != null)
                _winnerName = _players.FirstOrDefault(x => x.Value == score).Key.name;
        }
    }

    private void UpdateText()
    {
        _scoreText = "";
        foreach (var player in _players)
        {
            _scoreText += player.Key.name + ": " + player.Value + "\n";
        }
    }

    private void HandleScoreTextUpdated(string oldScoreText, string newScoreText)
    {
        OnScoreTextUpdated?.Invoke();
    }

    private void HandleWinnderNameUpdated(string oldWinnderName, string newWinnderName)
    {
        OnGameEnded?.Invoke();
    }
}