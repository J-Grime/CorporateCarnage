using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeshConfig : MonoBehaviour
{
    [SerializeField] private Transform _weaponAttachPoint;

    public Transform WeaponAttachPoint
        => _weaponAttachPoint;
}
