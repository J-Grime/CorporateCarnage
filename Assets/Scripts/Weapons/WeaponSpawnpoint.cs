using UnityEngine;

public class WeaponSpawnpoint : MonoBehaviour
{
    [SerializeField] private float _spawnChance = 0.5f;
    [SerializeField] private float _minWeaponQuality = 0;
    [SerializeField] private float _maxWeaponQuality;

    public float SpawnChance =>
        _spawnChance;

    public float MinWeaponQuality
        => _minWeaponQuality;

    public float MaxWeaponQuality
        => _maxWeaponQuality;

    // TODO: Set this to true if we have a weapon sitting idly
    public bool HasWeapon
        => this.Weapon != null;

    public GameObject Weapon { get; set; }

    public ParticleSystem SpawnParticles { get; set; }
}