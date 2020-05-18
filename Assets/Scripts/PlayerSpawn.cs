using Photon.Pun;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private bool _forceSpawnPlayer = false;
    [SerializeField] private string _defaultSpawnWeapon;
    
    // The path to the player's prefab, relative to the resources folder
    private string _playerPrefabPath = "Player";
    private PlayerController _instantiatedPlayer;

    internal TeamColor TeamColor =>
        _teamColor;

    internal TeamColor PlayerTeamColor
    {
        get
        {
            var playerList = PhotonNetwork.PlayerList;

            for (byte i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] == PhotonNetwork.LocalPlayer)
                {
                    return (TeamColor)i;
                }
            }

            return TeamColor.Yellow;
        }
    }

    private void Start()
    {
        if (_forceSpawnPlayer)
        {
            Spawn();
        }
    }

    internal PlayerController Spawn()
    {
        if (!PhotonNetwork.IsConnected) return null;

        if (this.PlayerTeamColor != this.TeamColor && !_forceSpawnPlayer)
        {
            return null;
        }

        _instantiatedPlayer = PhotonNetwork.Instantiate(_playerPrefabPath, this.transform.position, this.transform.rotation).GetComponent<PlayerController>();
        var playerController = _instantiatedPlayer.GetComponent<PlayerController>();

        PlayerController.LocalPlayerController = playerController;

        _instantiatedPlayer.gameObject.GetPhotonView().RequestOwnership();
        _instantiatedPlayer.gameObject.GetPhotonView().RPC(nameof(playerController.SetTeamColorRPC),
                                               RpcTarget.AllBufferedViaServer,
                                               (byte)_teamColor);

        if (!string.IsNullOrEmpty(_defaultSpawnWeapon))
        {
            PhotonNetwork.Instantiate($"Guns/{_defaultSpawnWeapon}", transform.position, transform.rotation);
        }

        return playerController;
    }

    private void Update()
    {
    }
}
