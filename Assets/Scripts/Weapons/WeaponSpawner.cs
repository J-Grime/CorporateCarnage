using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponSpawner : MonoBehaviourPun
{
    [SerializeField] private int _maxConcurrentWeapons = 4;
    [SerializeField] private int _minConcurrentWeapons = 1;
    [SerializeField] private float _weaponSpawnInterval = 5.0f;
    [SerializeField] private string[] _weaponPaths;
    [SerializeField] private string _pickupParticleSystem;

    private float _lastWeaponSpawned = 0;
    private WeaponSpawnpoint[] _spawnPoints;
    private List<GameObject> _weaponsActive = new List<GameObject>();

    private int WeaponsActiveCount
        => _weaponsActive.Count;

    internal void Awake()
    {
        _spawnPoints = FindObjectsOfType<WeaponSpawnpoint>();
    }

    internal void Update()
    {
        // If we aren't the master client then we shouldn't be processing weapons
        // The master client will handle this so it's fine
        if (!PhotonNetwork.IsMasterClient || _weaponPaths == null || !_weaponPaths.Any())
        {
            return;
        }


        var weaponsToRemove = _weaponsActive.Where(weapon => weapon == null).ToArray();
        foreach (var weapon in weaponsToRemove)
        {
            _weaponsActive.Remove(weapon);
        }

        var activeWeaponsCount = this.WeaponsActiveCount;

        if (activeWeaponsCount >= _maxConcurrentWeapons)
        {
            return;
        }

        if (activeWeaponsCount < _minConcurrentWeapons ||
            _lastWeaponSpawned + _weaponSpawnInterval < Time.time)
        {
            this.SpawnWeapons();
        }
    }

    internal void SpawnWeapons()
    {
        // Get all empty spawners, and the total spawn probability
        var emptySpawners = _spawnPoints.Where(sp => !sp.HasWeapon).ToArray();
        var combinedSpawnProbability = emptySpawners.Sum(spawner => spawner.SpawnChance);

        var weaponsActiveCount = this.WeaponsActiveCount;

        foreach (var spawner in emptySpawners)
        {
            // We want to scale the spawn chance to be between 0 and 1
            // This means if we have 10 spawns with a spawn chance of 15% that they'll each become 10%
            var scaledSpawnChance = spawner.SpawnChance / combinedSpawnProbability;
            var randNum = Random.Range(0, 1);

            if (randNum < scaledSpawnChance)
            {
                SpawnWeapon(spawner);
                weaponsActiveCount++;
            }

            if (weaponsActiveCount >= _maxConcurrentWeapons)
            {
                return;
            }
        }
    }

    private void SpawnWeapon(WeaponSpawnpoint spawnpoint)
    {
        // TODO: Take into account spawnpoint rarity
        var weaponName = _weaponPaths[Mathf.FloorToInt(Random.Range(0, _weaponPaths.Length))];
        var spawnTransform = spawnpoint.transform;

        var go = PhotonNetwork.Instantiate(weaponName,
                                           spawnTransform.position,
                                           Quaternion.identity);
        _weaponsActive.Add(go);

        spawnpoint.Weapon = go;
        _lastWeaponSpawned = Time.time;

        var spawnedGun = go.GetComponent<Gun>();
        var particleSystem = PhotonNetwork.Instantiate(_pickupParticleSystem, spawnTransform.position, Random.rotation);
        particleSystem.transform.parent = spawnedGun.transform;
        particleSystem.GetPhotonView().RequestOwnership();

        spawnedGun.OnWeaponPickedUp += () => PhotonNetwork.Destroy(particleSystem.gameObject);
    }
}