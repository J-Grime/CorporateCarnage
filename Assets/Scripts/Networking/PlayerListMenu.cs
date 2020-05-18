using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text[] _playerSlots;

    private List<Player> _players = new List<Player>();

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _players.Add(newPlayer);
        this.UpdateSlots();
        Debug.Log("Player entered");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _players.Remove(otherPlayer);
        this.UpdateSlots();
        Debug.Log("Player left");
    }
    
    private void Awake()
    {
        _players = PhotonNetwork.PlayerList.ToList();

        UpdateSlots();

        //PhotonNetwork.Room
    }


    private void UpdateSlots()
    {
        for (int slotIndex = 0; slotIndex < _playerSlots.Length; slotIndex++)
        {
            _playerSlots[slotIndex].text = (_players.Count > slotIndex) ? _players[slotIndex].NickName : "Not Connected";
        }
    }
}
