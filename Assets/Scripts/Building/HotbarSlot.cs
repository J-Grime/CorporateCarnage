using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private Image _selectedImage;
    [SerializeField] private Image _foregroundImage;

    private bool _selected;

    internal bool Selected
    {
        get => _selected;
        set
        {
            
            _selected = value;
            _foregroundImage.enabled = !value;
            _selectedImage.enabled = value;

            Debug.Log("selected");
        }
    }

    internal Sprite SelectedSprite
    {
        //gets the sprites
        get => _selectedImage.sprite;
        set => _selectedImage.sprite = value;
    }


    internal Sprite ForegroundSprite
    {
        //gets the sprites
        get => _foregroundImage.sprite;
        set => _foregroundImage.sprite = value;
    }

    private void Awake()
    {
        if (_selectedImage == null)
        {
            Debug.LogError($"The hotbar slot: {this.name} has no selected image element");
        }

        if (_foregroundImage == null)
        {
            Debug.LogError($"The hotbar slot: {this.name} has no foreground image element");
        }

    }
}
