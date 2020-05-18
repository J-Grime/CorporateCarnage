using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public struct WeaponFiredInfo
{
    public Vector3? hitPoint;
    public Vector3? hitNormal;
}

public class PlayerWeaponController : MonoBehaviourPun
{
    public delegate void ControllerUpdated(PlayerWeaponController controller);

    public static ControllerUpdated OnWeaponPickedUp;
    public static ControllerUpdated OnWeaponDestroyed;
    public static ControllerUpdated OnAmmoChanged;

    [SerializeField] private float _pickupDistance;

    public Camera playerCam;
    public ParticleSystem impactEffect;
    public GameObject _playerCanvas;

    private float nextTimeToFire;
    private Gun _activeGun;
    private bool _wasShootingLastFrame;
    private bool _recoil;
    private AudioSource _audioSource;
    private int _recoilBump;

    private PlayerAnimationController _playerAnimationController;
    private PlayerController _playerController;
    private PlayerMeshConfig _playerMeshConfig;

    private PhotonView _photonView;
    private Text reticle;
    private Vector3 originRet;
    private Vector3 ScreenCenter;
    
    private int _ammo;

    private float temp;

    internal Gun ActiveWeapon
        => _activeGun;

    internal int Ammo
    {
        get => _ammo;
        set
        {
            _ammo = value;

            if (_photonView.IsMine)
                OnAmmoChanged?.Invoke(this);
        }
    }

    public bool HasGun
        => _activeGun != null;

    internal void Start()
    {
        _playerAnimationController = GetComponent<PlayerAnimationController>();
        _playerController = GetComponent<PlayerController>();
        _photonView = GetComponent<PhotonView>();
        _audioSource = GetComponent<AudioSource>();


        if (_audioSource == null)
            _audioSource = this.gameObject.AddComponent<AudioSource>();

        reticle = GameObject.Find("Ret")?.GetComponent<Text>() ?? null;

        if (reticle != null)
        {
            originRet = reticle.transform.position;
            ScreenCenter = playerCam.ScreenToWorldPoint(reticle.transform.position);
        }
        else
        {
            Debug.Log("Error, no reticule could be found.");
        }
    }

    internal void Update()
    {
        if (_playerMeshConfig == null)
        {
            _playerMeshConfig = GetComponentInChildren<PlayerMeshConfig>();
            if (_playerMeshConfig != null)
            {
                var lossyScale = _playerMeshConfig.WeaponAttachPoint.transform.lossyScale;
                var newScale = new Vector3(1 / lossyScale.x, 1 / lossyScale.y, 1 / lossyScale.z);

                _playerMeshConfig.WeaponAttachPoint.transform.localScale = newScale;
            }
            return;
        }

        this.HandleRecoil();

        if (!_photonView.IsMine)
        {
            return;
        }

        if (Time.time- temp>1 &&(Input.GetButtonDown("Pickup")))
        {
            temp = Time.time;
            this.HandleWeaponPickups();
        }

        if (this.HasGun && !_playerAnimationController.IsSprinting)
        {
            this.HandleShooting();
        }

        if (reticle != null)
            reticle.transform.position = Vector3.Lerp(reticle.transform.position, originRet, 5.0f * Time.deltaTime);

        _wasShootingLastFrame = Input.GetAxis("Shoot") != 0;
    }

    internal void DestroyWeapon()
    {
        OnWeaponDestroyed?.Invoke(this);
        PhotonNetwork.Destroy(_activeGun.gameObject);

        _playerAnimationController.IsHoldingMinigun = false;
        _playerAnimationController.IsHoldingPistol = false;
    }

    private void HandleRecoil()
    {
        if (!this.HasGun)
        {
            _recoilBump = 0;
            return;
        }

        if (_recoilBump != 0)
        {
            _recoilBump += 1;
            _activeGun.transform.Rotate(Vector3.forward, 1.0f);
        }
        else if (_recoilBump == 0 && HasGun)
        {
            _activeGun.transform.forward = playerCam.transform.right;
        }
    }

    private void HandleShooting()
    {
        if (!this.HasGun ||
            Time.time < nextTimeToFire ||
            this.Ammo == 0 ||
            Input.GetAxis("Shoot") < 0.1)
        {
            return;
        }

        // If the player was shooting on the last frame and the weapon isn't automatic
        // They need to let go of the trigger before they can fire again
        if (_wasShootingLastFrame && !_activeGun.Automatic)
        {
            return;
        }

        nextTimeToFire = Time.time + (1f / _activeGun.RateOfFire);
        this.Shoot();
    }

    internal void HandleWeaponPickups()
    {
        var nearbyGun = GetNearbyGuns().FirstOrDefault();

        if (nearbyGun != null)
        {
            if (this.HasGun)
            {
                PhotonNetwork.Destroy(_activeGun.gameObject);
            }

            this.PickupWeapon(nearbyGun);
        }
    }

    private void Shoot()
    {
        Debug.Log("Begining of Shoot()");

        this.PlayEffects();
        _photonView.RPC(nameof(this.OnShootRPC), RpcTarget.Others);
        this.Ammo -= 1;

        var ray = new Ray(playerCam.transform.position + (playerCam.transform.forward / 2), playerCam.transform.forward);

        // Raycast frontally and check if we hit a player
        if (Physics.Raycast(ray,
                            out var initialRaycastHit,
                            _activeGun.Range))
        {
            var playerController = initialRaycastHit.transform.GetComponentInParent<PlayerController>();
            if (playerController != null && !playerController.IsDead)
            {

                // After doing an initial raycast, we want to fire a ray to find the player's bones
                var layerMask = LayerMask.NameToLayer("PlayerBones");
                var damage = (float)_activeGun.Damage;
                var damageMultiplier = 1.0f;

                if (Physics.Raycast(ray, out var bodypartHit, _activeGun.Range, 1 << layerMask))
                {
                    var bodypart = bodypartHit.collider.gameObject.GetComponent<PlayerBodypart>(); ;
                    damageMultiplier = bodypart?.DamageMultiplier ?? 1;
                    Debug.Log($"Raycast has hit a {bodypart.BodypartType}");
                }
                else
                {
                    Debug.Log($"Raycast has hit a player and not a body part");
                }

                var finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

                playerController.TakeDamage(finalDamage);

                var color = Color.Lerp(Color.green, Color.red, damageMultiplier / 1.5f);

                HitPopcornGenerator.Instance.GeneratePopcorn(initialRaycastHit.point, color, finalDamage);

                Debug.Log($"Dealt {_activeGun.Damage} damage to {playerController.name}");
            }
        }

        if (this.Ammo <= 0)
        {
            this.DestroyWeapon();
        }
    }

    private void PlayEffects()
    {
        if (_activeGun.MuzzleFlash != null)
            _activeGun.MuzzleFlash.Play();

        if (_activeGun.FireSound != null)
            _audioSource.PlayOneShot(_activeGun.FireSound);

        _recoilBump = -10;
        _activeGun.transform.Rotate(Vector3.forward, _recoilBump);
        _playerAnimationController.SetShooting();
        //if (_photonView.IsMine)
        //{
        //    reticle.transform.position += new Vector3(Random.Range(-5, 5), 15.0f, 0.0f);
        //}
    }

    private void PickupWeapon(Gun gun)
    {
        _activeGun = gun;

        this.Ammo = _activeGun.MagSize;
        gun.OnPickedUp();

        _playerMeshConfig.WeaponAttachPoint.transform.rotation = Quaternion.identity;

        Debug.Log("Rotation:" + (_playerMeshConfig.WeaponAttachPoint.localRotation.eulerAngles.ToString()));
        Debug.Log("Scale:" + (_playerMeshConfig.WeaponAttachPoint.localScale.ToString()));
        Debug.Log("POsition:" + (_playerMeshConfig.WeaponAttachPoint.localPosition.ToString()));


        gun.transform.parent = _playerMeshConfig.WeaponAttachPoint;



        gun.transform.localPosition = new Vector3(
            gun.HandleTransform?.transform.localPosition.x ?? 0,
            gun.HandleTransform?.transform.localPosition.y ?? 0,
            gun.HandleTransform?.transform.localPosition.z ?? 0);

        gun.transform.localRotation = Quaternion.identity;


        if (_photonView.IsMine)
        {
            gun.gameObject.GetPhotonView().RequestOwnership();

            _photonView.RPC(nameof(this.SetWeaponRPC), RpcTarget.Others, gun.gameObject.GetPhotonView().ViewID);

            OnWeaponPickedUp?.Invoke(this);

            PickupAlerts.Instance?.ShowPickup(gun.WeaponType);

            _playerAnimationController.IsHoldingMinigun = false;
            _playerAnimationController.IsHoldingPistol = false;

            switch (gun.WeaponType)
            {
                case WeaponType.Minigun:
                    _playerAnimationController.IsHoldingMinigun = true;
                    break;
                case WeaponType.Saucegun:
                case WeaponType.Pistol:
                    _playerAnimationController.IsHoldingPistol = true;
                    break;
            }
        }
    }

    private IEnumerable<Gun> GetNearbyGuns()
    {
        var collidersInRange = Physics.OverlapSphere(transform.position, _pickupDistance);

        // Get all colliders which are tagged as gun, and return the game object they're attached to
        var nearbyGuns = collidersInRange.Where(collider => collider.CompareTag("Gun"))
                                         .Select(collider => collider.gameObject.GetComponent<Gun>())
                                         .Where(gun => !gun.IsPickedUp);

        return nearbyGuns;
    }

    #region "Networked Functionality"

    [PunRPC]
    public void OnShootRPC()
    {
        this.PlayEffects();
    }

    [PunRPC]
    public void SetWeaponRPC(int gunPhotonViewId)
    {
        this.PickupWeapon(PhotonNetwork.GetPhotonView(gunPhotonViewId).gameObject.GetComponent<Gun>());
    }

    #endregion

}
