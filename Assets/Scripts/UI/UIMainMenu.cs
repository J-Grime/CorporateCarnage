using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Taken from the Photon tutorial series to help get started.

public class UIMainMenu : MonoBehaviourPunCallbacks
{
    private static UIMainMenu _instance;

    [SerializeField] private InputField _playerNameText;
    [SerializeField] private string _lobbySceneName = "Lobby";
    [SerializeField] private Text _isConnectedText;
    [SerializeField] private Button _connectButton;

    private UIRoomList _serverBrowser;

    private string _gameVersion = "1.0";
    private string[] _defaultNames =
    {
        "Gareth",
        "Simon",
        "Edward",
        "Marge",
        "Larry",
        "Lobster",
        "James",
        "John Crob",
        "Jennifer",
        "Enrique"
    };

    private bool _isConnected = false;

    internal static UIMainMenu Instance
        => _instance;

    internal bool IsConnected {
        get => _isConnected;
        set
        {
            if (_isConnectedText != null)
            {
                _isConnectedText.text = value ? "Online" : "Offline";
                _isConnectedText.color = value ? Color.green : Color.red;
            } else
            {
                Debug.LogError($"The field isConnectedText is not set on the game object {this.name} and therefore the player's connection status will not be shown");
            }

            if(_connectButton != null)
            {
                _connectButton.interactable = value;
            } else
            {
                Debug.LogError($"The field connectButton is not set on the game object {this.name} and therefore the player's connect button will be permanently grayed");
            }
        }
    }

    void Awake()
    {
        _instance = this;

        _serverBrowser = GetComponent<UIRoomList>();

        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.GameVersion = _gameVersion;
        PhotonNetwork.AuthValues = new AuthenticationValues()
        {
            AuthType = CustomAuthenticationType.None,
        };

        this.UpdateUsername();

        PhotonNetwork.ConnectUsingSettings();
    }

    public void HostGame()
    {
        var roomProperties = new Hashtable();
        roomProperties.Add(UIRoomRow.PROPERTY_NICKNAME, PhotonNetwork.NickName);

        var roomOptions = new RoomOptions()
        {
            IsVisible = true,
            MaxPlayers = 4,
            CustomRoomProperties = roomProperties,
            CustomRoomPropertiesForLobby = new string[] { UIRoomRow.PROPERTY_NICKNAME }
        };

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public void ConnectToRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
    }

    public void OpenServerBrowser()
    {
        _serverBrowser.Open();
        PhotonNetwork.GetCustomRoomList(TypedLobby.Default, "");
    }
    
    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public void UpdateUsername()
    {
        PhotonNetwork.NickName = string.IsNullOrWhiteSpace(_playerNameText?.text) ?
                    _defaultNames[Mathf.RoundToInt(Random.Range(0, _defaultNames.Length))] :
                    _playerNameText.text;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        this.IsConnected = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        this.IsConnected = false;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Launcher.cs#OnJoinedRoom()");

        // When joining a room, if we're the master client then that means that we hosted our own.
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(_lobbySceneName);
        }
    }

    public void QuitGame()
    {

#if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
