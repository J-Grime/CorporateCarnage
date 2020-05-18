using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerConfiguration))]
[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun, IOnEventCallback
{
    [SerializeField] private MonoBehaviour[] _controlledScripts;
    [SerializeField] private GameObject _flagOnBack;
    [SerializeField] private float _health = 50.0f;
    [SerializeField] private int _spawnDist;
    
    private Image _bloodImage;

    private PhotonView _photonView;
    private static PlayerController _localController;
    private Camera _camera;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private Transform _playerAnimatedBones;
    private Transform _playerRagdollBones;

    private Color lerpColor;

    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;

    private TeamColor _teamId;
    private bool _isHoldingFlag = false;
    private bool _inScoringZone = false;
    private float fadeFloat;
    Vignette vignetteLayer = null;
    PostProcessVolume _postProcess;

    GameObject _flag;

    public Vector3 _flagPosition;
    public Vector3 _spawnLocation;
    public bool _isFlagTouching;
    public bool _isDead;
    private bool _respawnWait;
    private float _timeOfDeath;

    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Collider _mainCollider;

    private bool _isInitialized = false;

    internal PlayerWeaponController WeaponController { get; private set; }

    internal TeamColor TeamId { 
        get => _teamId;
        set
        {
            _teamId = value;
            this.InitializePlayer();
        }
    }

    internal string NickName
    {
        get
        {
            if (!_photonView.IsOwnerActive) return "Unknown";
            return _photonView.Owner.NickName;
        }
    }

    internal bool IsHoldingFlag
    {
        get => _isHoldingFlag;
        set
        {
            if (_photonView.IsMine)
            {
                _photonView.RPC(nameof(this.SetHoldingFlag), RpcTarget.Others, value);
            }

            _isHoldingFlag = value;
            _flagOnBack.SetActive(value);
        }
    }

    internal bool IsDead
    {
        get => _isDead;
    }

    internal byte PlayerId
    {
        get
        {
            // List is lazily populated with LINQ so we need to cache it before iterating
            var playerList = PhotonNetwork.PlayerList;

            for (byte i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] == PhotonNetwork.LocalPlayer)
                {
                    return i;
                }
            }

            return 128;
        }
    }

    internal float Health
    {
        get => _health;
        set
        {
            _health = value;

            if (LocalPlayerController == this && IdentifiableObject.TryIdentify("HealthIndicator", out var healthIndicator))
            {
                healthIndicator.GetComponent<Text>().text = $"Health: {_health}";
            }
        }
    }

    internal float Score { get; private set; } = 0;

    internal static PlayerController LocalPlayerController { get; set; }

    internal void InitializePlayer()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        GameObject model = null;
        GameObject ragdoll = null;
        Vector3 offset = Vector3.zero;
        var config = GetComponent<PlayerConfiguration>();

        switch (_teamId) {
            case TeamColor.Yellow:
                model = config._yellowTeamModel;
                ragdoll = config._yellowTeamRagdoll;
                offset = config._yellowTeamOffset;
                break;

            case TeamColor.Green:
                model = config._greenTeamModel;
                ragdoll = config._greenTeamRagdoll;
                offset = config._greenTeamOffset;
                break;

            case TeamColor.Blue:
                model = config._blueTeamModel;
                ragdoll = config._blueTeamRagdoll;
                offset = config._blueTeamOffset;
                break;

            // Default to cynthia
            default:
                model = config._redTeamModel;
                ragdoll = config._redTeamRagdoll;
                offset = config._redTeamOffset;
                break;
        }

        _playerAnimatedBones = Instantiate(model, GetComponent<PlayerCamera>().RotationTarget).transform;
        _playerRagdollBones = Instantiate(ragdoll, transform).transform;

        _playerAnimatedBones.localPosition = offset;
        _playerRagdollBones.localPosition = offset;

        var collider = GetComponent<Collider>();
        foreach (var childCol in _playerRagdollBones.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(collider, childCol);
        }

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _animator = GetComponentInChildren<Animator>();


        var animatorView = GetComponent<PhotonAnimatorView>();
        
        animatorView.SetAnimator(_animator);

        for(int i = 0; i < _animator.layerCount; i++)
        {
            animatorView.SetLayerSynchronized(i, PhotonAnimatorView.SynchronizeType.Continuous);
        }

        foreach (var parameter in _animator.parameters)
        {
            PhotonAnimatorView.ParameterType paramType = PhotonAnimatorView.ParameterType.Bool;

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    paramType = PhotonAnimatorView.ParameterType.Bool;
                    break;

                case AnimatorControllerParameterType.Float:
                    paramType = PhotonAnimatorView.ParameterType.Float;
                    break;

                case AnimatorControllerParameterType.Int:
                    paramType = PhotonAnimatorView.ParameterType.Int;
                    break;

                case AnimatorControllerParameterType.Trigger:
                    paramType = PhotonAnimatorView.ParameterType.Trigger;
                    break;

            }

            animatorView.SetParameterSynchronized(parameter.name, paramType, PhotonAnimatorView.SynchronizeType.Continuous); ;
        }
    }

    void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        WeaponController = GetComponent<PlayerWeaponController>();
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
        _camera = GetComponentInChildren<Camera>();
        _rigidbody = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();

        if (Gamemode.Instance == null)
        {
            Debug.Log("No gamemode is loaded. Things will not work properly.");
        }
        else
        {
            Gamemode.Instance.PlayerControllers.Add(this);
        }
        _spawnLocation = this.transform.position;
        
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        _bloodImage = GameObject.Find("Blood")?.GetComponent<Image>();

        GameObject temp = GameObject.Find("Post-process Volume");

        _postProcess = temp.GetComponent<PostProcessVolume>();
        _postProcess.profile.TryGetSettings(out vignetteLayer);
        vignetteLayer.enabled.value = true;
    }

    void Update()
    {

        if (_isHoldingFlag)
        {
            _flagPosition = this.transform.position;
        }
        else
        {
            _flag = GameObject.FindGameObjectWithTag("Flag");
            _flagPosition = _flag?.transform.position ?? Vector3.zero;
        }

        if (!_photonView.IsMine)
        {
            _camera.gameObject.SetActive(false);
        }
        else
        {
            if (_isDead && Time.time - _timeOfDeath > 5)
            {
                _respawnWait = true;

            }
        }
        
        if (_respawnWait && Flag.Instance.leftZone)
        {
            Respawn();
        }
        else if (!_inScoringZone && _respawnWait)
        {
            Respawn();
        }
        //Debug.Log(_flagPosition);
        this.ProcessScoring();
    }

    internal void Respawn()
    {
        if (_photonView.IsMine)
        {
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;

            GameEvents.FireEvent(new PlayerRespawnEvent(this));
        }

        this.Health = 100;
        _respawnWait = false;
        _isDead = false;
        _timeOfDeath = 0;

        this.HideRagdoll();
    }

    internal void Die()
    {
        _timeOfDeath = Time.time;
        _isDead = true;

        if (_photonView.IsMine)
        {
            _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);

            if (_isHoldingFlag)
            {
                Flag.Instance.SetFlagStatus(true, transform.position);
                this.IsHoldingFlag = false;
            }

            if (WeaponController.HasGun)
            {
                this.WeaponController.DestroyWeapon();
            }

            GameEvents.FireEvent(new PlayerDeathEvent(this), new Photon.Realtime.RaiseEventOptions()
            {
                Receivers = Photon.Realtime.ReceiverGroup.Others
            });
        }

        this.ShowRagdoll();
    }

    private void ProcessScoring()
    {
        if (!_isHoldingFlag || !_inScoringZone) return;

        Score += Time.deltaTime;

        if (Score % 1 <= Time.deltaTime)
        {
            Debug.Log($"Player Score: {Mathf.RoundToInt(Score)}");
        }
    }

    public void TakeDamage(float amount)
        => this.TakeDamage(amount, true);

    private void TakeDamage(float amount, bool sendRPC)
    {
        if (_isDead) return;

        if (sendRPC)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC(nameof(this.TakeDamageRPC), RpcTarget.Others, amount);
        }

        Health -= amount;

        if (_photonView.IsMine) {
            StartCoroutine(damageScreen(0.1f, 1.0f, 3.0f));
        }

        //StartCoroutine(damageScreen(0.2f,0.5f));
        if (this.Health <= 0 && _photonView.IsMine)
        {
            this.Die();
        }
    }
    IEnumerator damageScreen(float FadeIntime, float _startTransparency,float FadeOutTime)
    {
        _bloodImage.enabled = true;

        StartCoroutine(fadeIN(_startTransparency,FadeOutTime));
        while (fadeFloat < _startTransparency)
        {
            _bloodImage.GetComponent<CanvasRenderer>().SetAlpha(fadeFloat);
            //vignetteLayer.color.value = Color.black;
            yield return new WaitForSeconds(FadeIntime/60);
            fadeFloat += 0.05f;
        }
        StartCoroutine(fadeOUT(FadeOutTime));
        while (fadeFloat > 0)
        {
            _bloodImage.GetComponent<CanvasRenderer>().SetAlpha(fadeFloat);
            yield return new WaitForSeconds(FadeOutTime/60);
            //vignetteLayer.color.value = Color.red;
        }

        fadeFloat = 0;
        _bloodImage.enabled = false;
    }

    private IEnumerator fadeOUT(float length)
    {
        float temp = fadeFloat;
        while (fadeFloat > 0.0f)
        {
            yield return new WaitForSeconds(temp/100);
            fadeFloat -= length;
        }
        
        fadeFloat = 0;
    }

    private IEnumerator fadeIN(float target, float length)
    {
        float temp = target;
        float temp2 = 0;
        while (temp2 < target)
        {
            yield return new WaitForSeconds(temp / 100);
            fadeFloat = temp2;
            temp2 += length;
        }

    }
    #region " Ragdolls "

    private void ShowRagdoll()
    {
        if (_playerAnimatedBones == null ||
            _playerRagdollBones == null)
        {
            Debug.LogError("Unable to show the ragdoll as it's not configured on the player");
            return;
        }

        this.UpdateRagdollBones();
        _skinnedMeshRenderer.rootBone = _playerRagdollBones;
        _skinnedMeshRenderer.bones = this.GetNewBones(_playerRagdollBones.GetComponentsInChildren<Transform>());
        _playerRagdollBones.gameObject.SetActive(true);
        _playerAnimatedBones.gameObject.SetActive(false);
        _mainCollider.enabled = false;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.useGravity = false;

        _animator.enabled = false;
    }

    private void HideRagdoll()
    {
        if (_playerRagdollBones == null)
        {
            Debug.LogError("Unable to hide the ragdoll as it's not configured on the player");
            return;
        }

        _playerRagdollBones.gameObject.SetActive(false);
        _playerAnimatedBones.gameObject.SetActive(true);
        _skinnedMeshRenderer.rootBone = _playerAnimatedBones;
        _skinnedMeshRenderer.bones = this.GetNewBones(_playerAnimatedBones.GetComponentsInChildren<Transform>());
        _animator.enabled = true;
        _mainCollider.enabled = true;

        _rigidbody.useGravity = true;
    }

    private Transform[] GetNewBones(IEnumerable<Transform> newBoneSet)
    {
        var newBones = new Transform[_skinnedMeshRenderer.bones.Length];
        var newBonesMap = this.MapTransformArray(newBoneSet);

        for (int boneIndex = 0; boneIndex < newBones.Length; boneIndex++)
        {
            var currentBoneName = _skinnedMeshRenderer.bones[boneIndex].name;
            newBones[boneIndex] = newBonesMap[currentBoneName];
        }

        return newBones;
    }

    private void UpdateRagdollBones()
    {
        var currentBonesMap = this.MapTransformArray(_skinnedMeshRenderer.bones);
        var transformsInRagdollMap = this.MapTransformArray(_playerRagdollBones.GetComponentsInChildren<Transform>());

        foreach (var ragdollBone in transformsInRagdollMap.Values)
        {
            if (currentBonesMap.TryGetValue(ragdollBone.name, out var correspondingTransform))
            {
                ragdollBone.transform.localPosition = correspondingTransform.localPosition;
                ragdollBone.transform.localRotation = correspondingTransform.transform.localRotation;

                Debug.Log($"Updating the transform {ragdollBone.name}");
            }
        }
    }

    private Dictionary<string, Transform> MapTransformArray(IEnumerable<Transform> transforms)
    {
        var dictionary = new Dictionary<string, Transform>();

        foreach (var transform in transforms)
        {
            dictionary[transform.name] = transform;
        }

        return dictionary;
    }

    #endregion


    #region " Collision Events "

    void OnCollisionEnter(Collision collision)
    {
        // If the player walked into the flag
        if (collision.collider.gameObject == Flag.Instance?.gameObject &&
            _photonView.IsMine &&
            !_isDead)
        {
            Flag.Instance.SetFlagStatus(false, Vector3.zero);
            this.IsHoldingFlag = true;
            Flag.Instance.holderTM = this.TeamId;
        }
    }
    void OnTriggerEnter(Collider collider)
    {
        if (this.TryGetScoringZone(collider.transform, out var scoringZone) &&
            scoringZone.ScoringTeam == this.TeamId)
        {
            _inScoringZone = true;
            Flag.Instance.leftZone = false;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (this.TryGetScoringZone(collider.transform, out var scoringZone) &&
            scoringZone.ScoringTeam == this.TeamId)
        {
            _inScoringZone = false;
        }

        if (scoringZone != null && scoringZone.ScoringTeam != this.TeamId)
        {
            Flag.Instance.leftZone = true;
        }

    }

    private bool TryGetScoringZone(Transform transform, out ScoringZone scoringZone)
    {
        scoringZone = null;

        if (!transform.CompareTag("scoring_zone"))
        {
            return false;
        }

        scoringZone = transform.GetComponent<ScoringZone>();

        return scoringZone != null;
    }

    #endregion

    #region " RPCs "

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == GameEvents.GetEventCode(typeof(PlayerDeathEvent)))
        {
            var eventData = new PlayerDeathEvent(photonEvent.CustomData);
            if (eventData.PlayerTeam == this.TeamId)
            {
                this.Die();
            }
        }
        else if (photonEvent.Code == GameEvents.GetEventCode(typeof(PlayerRespawnEvent)))
        {
            var eventData = new PlayerRespawnEvent(photonEvent.CustomData);
            if (eventData.PlayerTeam == this.TeamId)
            {
                this.Respawn();
            }
        }
    }

    [PunRPC]
    public void TakeDamageRPC(float amount)
        => this.TakeDamage(amount, false);

    [PunRPC]
    public void SetTeamColorRPC(byte teamColor)
        => this.TeamId = (TeamColor)teamColor;

    [PunRPC]
    public void SetHoldingFlag(bool holdingFlag)
        => this.IsHoldingFlag = holdingFlag;

    #endregion
}
