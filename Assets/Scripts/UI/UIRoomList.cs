using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRoomList : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _roomRoot;
    [SerializeField] private Transform _roomParent;
    [SerializeField] private UIRoomRow _roomRowPrefab;

    private UIRoomRow _header;
    private List<UIRoomRow> _roomRows = new List<UIRoomRow>();

    public void Awake()
    {
        this.Close();
    }

    public void Clear()
    {
        foreach(var row in _roomRows)
        {
            Destroy(row.gameObject);
        }

        if(_header != null)
            Destroy(_header);


        //var newRow = Instantiate(_roomRowPrefab, _roomParent.transform);
        //newRow.Name = "Host";
        //newRow.MapName = "Map Name";
        //newRow.SlotsText = "Players";
        //_header = newRow;

        _roomRows.Clear();
    }

    public void Close()
    {
        _roomRoot.gameObject.SetActive(false);
    }

    public void Open()
    {
        _roomRoot.gameObject.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        this.Clear();

        foreach(var room in roomList)
        {
            var newRow = Instantiate(_roomRowPrefab, _roomParent.transform);

            var rectTransform = newRow.gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -(_roomRows.Count) * rectTransform.sizeDelta.y);

            newRow.UpdateFromRoomInfo(room);
            newRow.RoomInfo = room;

            _roomRows.Add(newRow);
        }
    }

}
