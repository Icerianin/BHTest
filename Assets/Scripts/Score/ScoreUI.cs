using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private void OnEnable() => PlayersScoreContainer.OnScoreTextUpdated += UpdateUI;

    private void OnDisable() => PlayersScoreContainer.OnScoreTextUpdated -= UpdateUI;

    private void UpdateUI()
    {
        _scoreText.text = FindObjectOfType<PlayersScoreContainer>().ScoreText;
    }
}