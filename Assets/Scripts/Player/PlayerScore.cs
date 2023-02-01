using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerScore : NetworkBehaviour
{
    [SerializeField] private PlayerDash _playerDash;
    private PlayersScoreContainer _scoreDisplayer;

    [SyncVar(hook = nameof(HandleScoreUpdated))]
    private int _curScore = 0;

    [ClientRpc]
    public void Setup(PlayersScoreContainer scoreDisplayer) => _scoreDisplayer = scoreDisplayer;

    [ClientCallback]
    private void OnEnable()
    {
        if (_scoreDisplayer)
            _scoreDisplayer.AddPlayer(gameObject);

        _playerDash.OnScoreChanged += OnScoreChanged;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (_scoreDisplayer)
            _scoreDisplayer.RemovePlayer(gameObject);

        _playerDash.OnScoreChanged -= OnScoreChanged;
    }

    [Client]
    private void OnScoreChanged(int newScore) => _curScore = newScore;

    private void HandleScoreUpdated(int oldScore, int newScore)
    {
        if (_scoreDisplayer)
            _scoreDisplayer.ChangeScoreForPlayer(gameObject, newScore);
    }
}