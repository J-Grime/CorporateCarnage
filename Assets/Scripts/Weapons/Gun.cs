using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Gun : MonoBehaviour
{
    public delegate void WeaponPickedUpEvent();
    public WeaponPickedUpEvent OnWeaponPickedUp;

    [SerializeField] private WeaponType _weaponType;

    [SerializeField] private Transform _handleTransform;

    [SerializeField] private AudioClip[] _fireSounds;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private ParticleSystem _impactFlash;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletEjectionForce = 1000.0f;

    [SerializeField] private int _rateOfFire;
    [SerializeField] private int _magSize;
    [SerializeField] private int _range;
    [SerializeField] private int _damage;
    [SerializeField] private bool _automatic;
    [SerializeField] private GameObject _bullet;

    private PhotonView _photonView;
    private bool _pickedUp = false;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private float _rotationOffset = 0.0f;

    void Awake()
    {
        _originalPosition = transform.localPosition;
        _originalScale = transform.localScale;
        _originalRotation = transform.rotation;
    }

    internal void Start()
    {
        _photonView = GetComponent<PhotonView>();

        _rotationOffset = Random.value * 360;
        transform.localScale = _originalScale * 2;
    }

    internal void Update()
    {
        if (_pickedUp) return;
        transform.position = _originalPosition + new Vector3(0, 0.2f + (Mathf.Sin(Time.time + _rotationOffset) / 5), 0);
        transform.rotation = Quaternion.Euler(0, (Time.time + _rotationOffset) * 32, -30);
    }

    public bool IsPickedUp
        => _pickedUp;

    public Transform HandleTransform
        => _handleTransform;

    public WeaponType WeaponType
        => _weaponType;

    internal AudioClip FireSound
        => _fireSounds == null ? null : _fireSounds[Mathf.FloorToInt(UnityEngine.Random.Range(0, _fireSounds.Length - 0.01f))];

    internal ParticleSystem MuzzleFlash => _muzzleFlash;

    internal ParticleSystem ImpactParticles => _impactFlash;

    internal GameObject BulletPrefab => _bulletPrefab;

    internal float RateOfFire => _rateOfFire;

    internal bool Automatic => _automatic;

    internal int Range => _range;

    internal int MagSize => _magSize;

    internal int Damage => _damage;

    internal void OnPickedUp()
    {
        if (_pickedUp) return;

        _pickedUp = true;
        OnWeaponPickedUp?.Invoke();

        transform.localPosition = Vector3.zero;
        transform.localScale = _originalScale;
        transform.localRotation = Quaternion.identity;
    }
}