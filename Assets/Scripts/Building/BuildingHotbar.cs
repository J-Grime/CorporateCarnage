using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingHotbar : MonoBehaviour
{
    public delegate void HotbarSlotChanged(int newHotbarSlot);
    public static event HotbarSlotChanged OnHotbarSlotChanged;

    [SerializeField] private int _slotSpacing = 10;

    [SerializeField] private int _selectedSlot = 0;
    [SerializeField] private int _setFalse = 0;

    [SerializeField] private HotbarSlot _slotPrefab;

    [SerializeField] private Sprite[] _slotSprites;
    [SerializeField] private Sprite[] _backgroundSprites;
    
    private HotbarSlot[] _slots;
    private HotbarSlot[] _background;
    private RectTransform _rectTransform;

    internal int CorrectCanvasWidth
        => _slotSpacing + (_slotSprites.Length * (_slotSpacing));

    internal int CorrectCanvasHeight
        => 200;

    internal int SelectedSlot
    {
        get => _selectedSlot;
        set
        {
            if (_selectedSlot == value) return;

            _slots[_selectedSlot].Selected = false;
            _slots[value].Selected = true;
            _selectedSlot = value;

            OnHotbarSlotChanged?.Invoke(value);
        }
    }


    private void Awake()
    {
        this.CleanupChildren();

        _rectTransform = this.GetComponent<RectTransform>();
        _slots = new HotbarSlot[_slotSprites.Length];
        _background = new HotbarSlot[_backgroundSprites.Length];
        _rectTransform.sizeDelta = new Vector2(this.CorrectCanvasWidth*1.5f, this.CorrectCanvasHeight);

        _rectTransform.ForceUpdateRectTransforms();
        for (var i = 0; i < _slots.Length; i++)
        {
            this.CreateSlot(i);
        }
    }
    private void Update()
    {
        for (int i = 1; i < _slotSprites.Length + 1; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                this.SelectedSlot = i - 1;
            }
            
        }
    }

    void CreateSlot(int slot)
    {
        var maxSlots = _slotSprites.Length;
        var xPos = 0;
        if(maxSlots % 2 == 0)
        {
            xPos = ((slot - (_slotSprites.Length / 2)) * _slotSpacing);
        }
        else
        {
            xPos = ((slot - ((_slotSprites.Length-1) / 2)) * _slotSpacing);
        }

        var createdSlot = Instantiate(_slotPrefab, transform);
        createdSlot.Selected = slot == _selectedSlot;
        
        createdSlot.ForegroundSprite = _slotSprites[slot];
        createdSlot.SelectedSprite = _backgroundSprites[slot];
                
        createdSlot.name = $"Hotbar Slot {slot}";

        createdSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            xPos,
            0);
        _slots[slot] = createdSlot;
    }

    void CleanupChildren()
    {
        // Cleanup slots if they exist
        if (_slots != null)
        {
            foreach (var slot in _slots)
            {
                Destroy(slot);
            }

            _slots = null;
        }

        // Cleanup slots which are in the tree heirarchy but which aren't in _slots
        var hotbarSlots = transform.GetComponentsInChildren<HotbarSlot>();
        foreach (var hotbarSlot in hotbarSlots)
        {
            DestroyImmediate(hotbarSlot?.gameObject);
        }
    }
}
