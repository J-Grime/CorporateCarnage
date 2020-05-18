using UnityEngine;

public enum Bodypart
{
    Head,
    Body,
    Arm,
    Leg
}

public class PlayerBodypart : MonoBehaviour
{
    [SerializeField] private Bodypart _bodypart;

    internal PlayerController PlayerController { get; private set; }

    internal Bodypart BodypartType
        => _bodypart;

    internal float DamageMultiplier
    {
        get
        {
            switch (this.BodypartType) {
                case Bodypart.Arm:
                case Bodypart.Leg:
                    return 0.5f;
                case Bodypart.Body:
                    return 1.0f;
                case Bodypart.Head:
                    return 1.5f;
                default:
                    return 1.0f;
            }
        }
    }


    void Awake()
    {
        this.PlayerController = GetComponentInParent<PlayerController>();
    }
}
