using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal enum ImageFadeState
{
    None,
    FadingIn,
    FadingOut,
    StayVisible
}

public class PickupAlerts : MonoBehaviour
{
    [SerializeField] private bool _forceRefresh = false;
    [SerializeField] private float _fadeInTime = 1.0f;
    [SerializeField] private float _fadeOutTime = 2.0f;
    [SerializeField] private float _stickAroundTime = 2.0f;

    [SerializeField] private Sprite _nailgunSprite;
    [SerializeField] private Sprite _pistolSprite;
    [SerializeField] private Sprite _minigunSprite;

    [SerializeField] private Image _displayImage;

    private static Dictionary<WeaponType, Sprite> sWeaponTypeImages = new Dictionary<WeaponType, Sprite>();

    public static PickupAlerts Instance { get; private set; }

    private float _stateLastChanged = 0;
    private ImageFadeState _fadeState = ImageFadeState.None;

    internal void Awake()
    {
        Instance = this;

        if (_displayImage == null)
        {
            _displayImage = GetComponentInChildren<Image>();
        }

        sWeaponTypeImages.Clear();

        sWeaponTypeImages.Add(WeaponType.Saucegun, _nailgunSprite);
        sWeaponTypeImages.Add(WeaponType.Minigun, _minigunSprite);
        sWeaponTypeImages.Add(WeaponType.Pistol, _pistolSprite);
    }

    internal void Update()
    {
        if (_forceRefresh)
        {
            _forceRefresh = false;
            this.ShowPickup(WeaponType.Saucegun);
            return;
        }

        if (_fadeState == ImageFadeState.None)
        {
            return;
        }

        var timeSinceState = Time.time - _stateLastChanged;
        var col = _displayImage.color;

        switch (_fadeState)
        {
            case ImageFadeState.FadingIn:

                _displayImage.color = new Color(col.r, col.g, col.b, (timeSinceState / _fadeInTime));

                if (timeSinceState > _fadeInTime)
                {
                    _fadeState = ImageFadeState.StayVisible;
                    _stateLastChanged = Time.time;
                    return;
                }

                break;
            case ImageFadeState.FadingOut:
                _displayImage.color = new Color(col.r, col.g, col.b, 1 - (timeSinceState / _fadeOutTime));

                if (timeSinceState > _fadeOutTime)
                {
                    _fadeState = ImageFadeState.None;
                    return;
                }

                break;
            case ImageFadeState.StayVisible:

                if (timeSinceState > _stickAroundTime)
                {
                    _fadeState = ImageFadeState.FadingOut;
                    _stateLastChanged = Time.time;
                    return;
                }
                
                break;
        }
    }

    internal void ShowPickup(WeaponType type)
    {
        if (!sWeaponTypeImages.TryGetValue(type, out var sprite) || sprite == null)
        {
            return;
        }

        _displayImage.overrideSprite = sprite;
        _stateLastChanged = Time.time;
        _fadeState = ImageFadeState.FadingIn;
    }
}
