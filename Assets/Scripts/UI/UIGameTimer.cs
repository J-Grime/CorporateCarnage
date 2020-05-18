using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameTimer : MonoBehaviour
{
    [SerializeField] private Text _timerText;

    private void Update()
    {
        if (Gamemode.Instance == null) return;

        _timerText.text = FormatTimeRemaining();
    }

    private string FormatTimeRemaining()
    {
        var timeRemaining = Gamemode.Instance.PhaseTimeRemaining;

        var minutes = Mathf.FloorToInt(timeRemaining / 60);
        var seconds = Mathf.FloorToInt(timeRemaining - (minutes * 60));

        var secondsText = (seconds < 10 ? $"0{seconds}" : seconds.ToString());

        return $"{minutes}:{secondsText}";
    }
}
