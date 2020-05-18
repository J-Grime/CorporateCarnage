using UnityEngine;

public enum StructurePlacement
{
    None,
    OnEdge,
    OverTile,
}

public class Structure : MonoBehaviour
{
    [SerializeField] private int _materialCost = 1;
    [SerializeField] private bool _ghosted = false;
    [SerializeField] private Material _ghostMaterial;
    [SerializeField] private Collider _collider;
    [SerializeField] private Renderer _meshRenderer;
    [SerializeField] private StructurePlacement _structurePlacement;

    // How big the 
    [SerializeField] private int _structureGridWidth = 1;
    [SerializeField] private int _structureGridHeight = 2;

    private Material[] _previousMaterials;

    // Used to determine whether we've manually ghosted in-editor
    private bool _isMeshGhosted = false;

    internal int MaterialCost
        => _materialCost;

    public StructurePlacement StructurePlacement
        => _structurePlacement;

    public int GridWidth
        => _structureGridWidth;

    public int GridHeight
        => _structureGridHeight;

    public bool Ghosted
    {
        get => _ghosted;
        set
        {
            if (value == _isMeshGhosted)
            {
                return;
            }

            _ghosted = value;
            _isMeshGhosted = value;

            if (_meshRenderer == null)
            {
                Debug.LogError($"Attempted to change the ghosting of: {this.name} when it does not have a renderer assigned...");
                return;
            }

            if (!_ghosted)
            {
                // If we are un-ghosting then restore the materials to what they should be
                _meshRenderer.materials = _previousMaterials;
                _previousMaterials = null;
                _collider.enabled = true;
            }
            else
            {
                // If we are ghosting then we need to store all of the materials.
                _previousMaterials = new Material[_meshRenderer.materials.Length];
                _collider.enabled = false;

                var newMaterials = new Material[_meshRenderer.materials.Length];

                for (var i = 0; i < _previousMaterials.Length; i++)
                {
                    _previousMaterials[i] = _meshRenderer.materials[i];
                    newMaterials[i] = _ghostMaterial;
                }
                _meshRenderer.materials = newMaterials;

            }
        }
    }


    void Awake()
    {
        if (_collider == null)
        {
            _collider = GetComponentInChildren<Collider>();
        }

        this.Ghosted = _ghosted;
    }

    private void Update()
    {
        this.Ghosted = _ghosted;
    }
}
