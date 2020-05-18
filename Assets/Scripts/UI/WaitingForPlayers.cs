using UnityEngine;

public class WaitingForPlayers : MonoBehaviour
{
    void Awake()
    {
        Gamemode.Instance.OnGameStateChanged += GameStateChanged;
    }

    private void OnDestroy()
    {
        Gamemode.Instance.OnGameStateChanged -= GameStateChanged;
    }

    void GameStateChanged(GameState oldGameState, GameState newGameState)
    {
        switch (newGameState)
        {
            case GameState.PreRound:
                this.SetChildrenEnabled(true);
                break;
            default:
                this.SetChildrenEnabled(false);
                break;
        }
    }

    void SetChildrenEnabled(bool enabled)
    {
        for (var i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(enabled);
        }
    }
}
