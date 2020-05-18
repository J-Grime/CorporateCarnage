using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRoomRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{

    public const string PROPERTY_NICKNAME = "host_nickname";

    [SerializeField] private Color _hoveredColor = Color.gray;
    [SerializeField] private Text _serverNameText;
    [SerializeField] private Text _serverMapText;
    [SerializeField] private Text _serverSlotsText;

    private Color _defaultColor;
    private Image _image;
    private bool _isMouseOver = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _defaultColor = _image.color;
    }

    public RoomInfo RoomInfo { get; set; }

    public string Name
        => (string)this.RoomInfo.CustomProperties[UIRoomRow.PROPERTY_NICKNAME];

    public void UpdateFromRoomInfo(RoomInfo info)
    {
        _serverNameText.text = (string)info.CustomProperties[UIRoomRow.PROPERTY_NICKNAME];
        _serverSlotsText.text = $"{info.PlayerCount}|{info.MaxPlayers}";
        _serverMapText.text = "Office";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.color = _hoveredColor;
        _isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.color = _defaultColor;
        _isMouseOver = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UIMainMenu.Instance.ConnectToRoom(this.RoomInfo);
    }
}
