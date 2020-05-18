using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAmmoController : MonoBehaviour
{
    private static Dictionary<WeaponType, Sprite> _ammoImageSprites = new Dictionary<WeaponType, Sprite>();

    [SerializeField] private Image _ammoImage;
    [SerializeField] private Text _ammoText;

    [SerializeField] private Sprite _pistolSprite;
    [SerializeField] private Sprite _saucegunSprite;
    [SerializeField] private Sprite _minigunSprite;

    internal void OnEnable()
    {
        PlayerWeaponController.OnWeaponDestroyed += OnWeaponDestroyed;
        PlayerWeaponController.OnWeaponPickedUp += OnWeaponPickedUp;
        PlayerWeaponController.OnAmmoChanged += OnAmmoChanged;

        _ammoImage.gameObject.SetActive(false);
        _ammoText.gameObject.SetActive(false);

        _ammoImageSprites.Clear();

        _ammoImageSprites.Add(WeaponType.Pistol, _pistolSprite);
        _ammoImageSprites.Add(WeaponType.Saucegun, _saucegunSprite);
        _ammoImageSprites.Add(WeaponType.Minigun, _minigunSprite);
    }

    internal void OnDisable()
    {
        PlayerWeaponController.OnWeaponDestroyed -= OnWeaponDestroyed;
        PlayerWeaponController.OnWeaponPickedUp -= OnWeaponPickedUp;
        PlayerWeaponController.OnAmmoChanged -= OnAmmoChanged;

    }

    private void OnWeaponDestroyed(PlayerWeaponController controller)
    {
        _ammoImage.gameObject.SetActive(false);
        _ammoText.gameObject.SetActive(false);
    }

    private void OnWeaponPickedUp(PlayerWeaponController controller)
    {
        _ammoImage.gameObject.SetActive(true);
        _ammoText.gameObject.SetActive(true);

        _ammoImage.sprite = _ammoImageSprites[controller.ActiveWeapon.WeaponType];
        _ammoText.text = controller.Ammo.ToString();
    }

    private void OnAmmoChanged(PlayerWeaponController controller)
    {
        _ammoText.text = controller.Ammo.ToString();
    }
}
