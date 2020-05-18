using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyButtons : MonoBehaviourPunCallbacks
{

    [SerializeField] private string _gameLevelName = "TestNetLevel";
    [SerializeField] private string _menuLevelName = "MainMenu";

    [SerializeField] private Button _startGameButton;

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(_gameLevelName);
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void LeaveGame()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene(_menuLevelName);
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(_menuLevelName);
    }

    private void Update()
    {
        _startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }
}
