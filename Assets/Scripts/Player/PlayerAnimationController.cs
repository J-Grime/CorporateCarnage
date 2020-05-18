using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    private const string HOLDING_MINIGUN_KEY = "IsHoldingMinigun";
    private const string HOLDING_PISTOL_KEY = "IsHoldingPistol";
    private const string IS_SPRINTING_KEY = "IsSprinting";
    private const string IS_STRAFING_KEY = "IsStrafing";
    private const string IS_MOVING_KEY = "IsMovingForwardsOrBackwards";
    private const string FORWARDS_SPEED_KEY = "ForwardsSpeed";
    private const string RIGHT_SPEED_KEY = "RightSpeed";

    private const string WEAPON_LAYER_NAME = "Weapon";
    private const float SPRINT_TRANSITION_MULT = 4.0f;

    private Animator _animator;

    public bool IsReady
        => _animator != null;

    public bool IsHoldingMinigun
    {
        get => _animator?.GetBool(HOLDING_MINIGUN_KEY) ?? false;
        set => this.SetAnimatorBool(HOLDING_MINIGUN_KEY, value);
    }

    public bool IsHoldingPistol
    {
        get => _animator?.GetBool(HOLDING_PISTOL_KEY) ?? false;
        set => this.SetAnimatorBool(HOLDING_PISTOL_KEY, value);
    }

    public bool IsSprinting
    {
        get => _animator?.GetBool(IS_SPRINTING_KEY) ?? false;
        set
        {
            this.SetAnimatorBool(IS_SPRINTING_KEY, value);
        }
    }

    public bool IsStrafing
    {
        get => _animator?.GetBool(IS_STRAFING_KEY) ?? false;
        set => this.SetAnimatorBool(IS_STRAFING_KEY, value);
    }

    public bool IsMovingBackwardsOrForwards
    {
        get => _animator?.GetBool(IS_MOVING_KEY) ?? false;
        set => this.SetAnimatorBool(IS_MOVING_KEY, value);
    }

    public float ForwardsSpeed
    {
        get => _animator?.GetFloat(FORWARDS_SPEED_KEY) ?? 0;
        set => this.SetFloat(FORWARDS_SPEED_KEY, value);
    }

    public float RightSpeed
    {
        get => _animator?.GetFloat(RIGHT_SPEED_KEY) ?? 0;
        set => this.SetFloat(RIGHT_SPEED_KEY, value);
    }

    public void SetShooting()
    {
        _animator?.SetTrigger("ShootTrigger");
    }

    private void UpdateLayerWeights()
    {
        var layerId = _animator.GetLayerIndex(WEAPON_LAYER_NAME);
        var currentWeight = _animator.GetLayerWeight(layerId);

        if (this.IsSprinting || (!this.IsHoldingMinigun && !this.IsHoldingPistol))
        {
            _animator.SetLayerWeight(layerId, Mathf.Max(0f, currentWeight - (Time.deltaTime * SPRINT_TRANSITION_MULT)));
        } else
        {
            _animator.SetLayerWeight(layerId, Mathf.Min(1f, currentWeight + (Time.deltaTime * SPRINT_TRANSITION_MULT)));
        }
    }

    private void SetFloat(string key, float value)
    {
        if (_animator == null) return;
        if (_animator.GetFloat(key) == value) return;

        _animator.SetFloat(key, value);
    }

    private void SetAnimatorBool(string key, bool value)
    {
        if (_animator == null) return;
        if (_animator.GetBool(key) == value) return;

        _animator.SetBool(key, value);
        this.UpdateLayerWeights();
    }

    void Start()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
    }

    void Update()
    {

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        this.UpdateLayerWeights();
    }
}
