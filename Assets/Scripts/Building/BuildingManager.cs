using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class BuildingManager : MonoBehaviourPun
{
    private List<Structure> _structures = new List<Structure>();

    private PhotonView _photonView;
    
    internal void Awake()
    {
        _photonView = this.GetComponent<PhotonView>();
    }

    internal void PlaceStructure(Structure structure, Vector3 position, Quaternion rotation)
    {
        var newStructure = Instantiate<Structure>(structure, position, rotation);

        _structures.Add(newStructure);
    }

}
