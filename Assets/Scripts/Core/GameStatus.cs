using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameStatus : NetworkBehaviour
{
    [SerializeField] private float _timeToRestart = 5f;
    [SerializeField] private TMP_Text _winnerDisplayerText;

    [ClientCallback]
    private void OnEnable()
    {
        _winnerDisplayerText.enabled = false;
        PlayersScoreContainer.OnGameEnded += OnGameEndedClient;
    }

    public override void OnStartServer()
    {
        PlayersScoreContainer.OnGameEnded += OnGameEndedServer;
    }

    private void OnDisable()
    {
        PlayersScoreContainer.OnGameEnded -= OnGameEndedServer;
        PlayersScoreContainer.OnGameEnded -= OnGameEndedClient;
    }

    [Client]
    private void OnGameEndedClient()
    {
        _winnerDisplayerText.enabled = true;
        _winnerDisplayerText.text = "Победитель: " + FindObjectOfType<PlayersScoreContainer>().WinnerName
            + "\nСкоро начнется новый матч!";
    }

    [Server]
    private void OnGameEndedServer()
    {
        StartCoroutine(TimerToReloadGame());
    }

    private IEnumerator TimerToReloadGame()
    {
        yield return new WaitForSeconds(_timeToRestart);
        NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
    }
}