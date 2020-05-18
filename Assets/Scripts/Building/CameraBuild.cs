using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBuild : MonoBehaviourPun
{
    [SerializeField] private Structure[] _structures;
    [SerializeField] private int _structureId = 0;
    [SerializeField] private string _buildRegionTag = "build_region";

    private bool _rotatingLastFrame = false;
    private int _rotationFrame = 0;

    private Structure _ghostedInstance;

    private PhotonView _photonView;

    private float _lastCameraMoved;
    private Camera _buildingCamera;

    internal Structure SelectedPrefab
        => _structures[_structureId];

    internal MaterialsRemainingCounter MaterialsCounter =>
        MaterialsRemainingCounter.Instance;

    private Vector3 _lastRaycastPosition;
    private BuildingZone _buildingZone;

    internal int MaterialsRemaining
    {
        get => this.MaterialsCounter?.MaterialsRemaining ?? int.MinValue;
        set => this.MaterialsCounter.MaterialsRemaining = value;
    }

    void Start()
    {
        _buildingCamera = GetComponentInChildren<Camera>();
        _photonView = this.GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        BuildingHotbar.OnHotbarSlotChanged += OnHotbarSlotChanged;
    }

    private void OnDisable()
    {
        BuildingHotbar.OnHotbarSlotChanged -= OnHotbarSlotChanged;
    }

    void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        this.ProcessRotationInput();

        if (!this.TryCameraRaycast(out var hit))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            this.PlaceStructure();
            this.CreateNewGhost();
        }

        this.UpdateGhostTransform();
    }

    internal void PlaceStructure()
    {
        if (_ghostedInstance != null)
        {
            _ghostedInstance.Ghosted = false;

            //switch (_ghostedInstance.StructurePlacement) {
            //    case StructurePlacement.OnEdge:
            //        //_buildingZone.GetTile()
            //        break;
            //    case StructurePlacement.OverTile:
            //        break;
            //    case StructurePlacement.None:
            //        break;
            //}


            _photonView.RPC("PlaceStructureRPC",
                            RpcTarget.Others,
                            _structureId,
                            _ghostedInstance.transform.position,
                            _ghostedInstance.transform.rotation);

            _ghostedInstance = null;
        }
    }

    internal void AbortStructure()
    {
        if (_ghostedInstance != null)
        {
            this.MaterialsRemaining += 1;
            Destroy(_ghostedInstance.gameObject);
        }
    }

    internal void CreateNewGhost()
    {
        if (_ghostedInstance != null)
        {
            Destroy(_ghostedInstance.gameObject);
            this.MaterialsRemaining += 1;
        }

        if (this.MaterialsRemaining < this.SelectedPrefab.MaterialCost)
        {
            Debug.Log("Not enough materials!");
            return;
        }

        this.MaterialsRemaining -= this.SelectedPrefab.MaterialCost;

        // Place down the structure and ghost it
        _ghostedInstance = Instantiate(this.SelectedPrefab, _buildingZone.GetNearestEdge(_lastRaycastPosition), this.GetIntendedRotation());
        _ghostedInstance.Ghosted = true;
    }

    internal void OnHotbarSlotChanged(int newSlot)
    {
        _structureId = newSlot;
        this.CreateNewGhost();
    }

    [PunRPC]
    internal void PlaceStructureRPC(int structureId, Vector3 position, Quaternion rotation)
    {
        // Place a structure down with the specified position and rotation.
        // This should only be called on other clients so we don't need to worry about that.

        Instantiate(_structures[structureId], position, rotation);
    }

    private void ProcessRotationInput()
    {
        var rotationRaw = Input.GetAxisRaw("RotateBuilding");
        var rotatingNow = rotationRaw != 0f;

        if (_rotatingLastFrame && rotatingNow)
        {
            return;
        }

        _rotatingLastFrame = rotatingNow;

        if (rotatingNow)
        {
            _rotationFrame += Mathf.RoundToInt(rotationRaw);
        }
    }

    private Quaternion GetIntendedRotation()
        => Quaternion.Euler(0, _rotationFrame * 90, 0);

    private void UpdateGhostTransform()
    {
        if (_ghostedInstance == null) return;

        _ghostedInstance.transform.rotation = Quaternion.Euler(0, _rotationFrame * 90, 0);

        if (_buildingZone != null)
        {
            if (_buildingZone.CanPlaceStructure(_structures[_structureId], _lastRaycastPosition, _rotationFrame, out var placePos, out var desiredTile))
            {
                _ghostedInstance.transform.position = placePos;
            }
        }
    }

    private bool TryCameraRaycast(out RaycastHit hit)
    {
        var camTransform = _buildingCamera.transform;

        var success = Physics.Raycast(camTransform.position, camTransform.forward, out hit);
        var validHit = success && this.IsHitValid(hit);

        if (validHit)
        {
            _lastRaycastPosition = hit.point;
            _buildingZone = hit.collider.gameObject.GetComponent<BuildingZone>();
        }

        Debug.DrawRay(camTransform.position, camTransform.forward * 100, validHit ? Color.green : success ? Color.blue : Color.red);

        return success && validHit;
    }

    private bool IsHitValid(RaycastHit hit)
        => hit.collider.gameObject.CompareTag("build_region");
}
