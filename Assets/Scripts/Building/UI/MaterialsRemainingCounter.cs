using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MaterialsRemainingCounter : MonoBehaviour
{
    [SerializeField] private Text _text;
    [SerializeField] private int _maxMaterials = 20;
    [SerializeField] private int _materialsRemaining = 20;

    // Used to tracker whether the counter was changed from the UI or via the property
    // If we don't have this, it won't update in-editor
    private int _lastMaxMaterials = 0;
    private int _lastMaterialsRemaining = 0;

    internal static MaterialsRemainingCounter Instance { private set; get; }

    internal int MaxMaterials
    {
        get => _maxMaterials;
        set
        {
            if (value == _lastMaxMaterials) return;

            _lastMaxMaterials = value;
            _maxMaterials = value;

            UpdateText();
        }
    }

    internal int MaterialsRemaining
    {
        get => _materialsRemaining;
        set
        {
            if (value == _lastMaterialsRemaining) return;
            _lastMaterialsRemaining = value;
            _materialsRemaining = value;

            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (_text != null)
        {
            _text.text = $"{_materialsRemaining} / {_maxMaterials}";
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one MaterialsRemainingCounter classes were initialized. This can cause unintended behaviours");
            return;
        }

        Instance = this;

        if (_text == null)
        {
            _text = GetComponentInChildren<Text>();
        }

        if (_text == null)
        {
            Debug.LogError("Unable to set remaining materials as we couldn't find any Text objects");
        }
    }

    private void Update()
    {
        this.MaterialsRemaining = _materialsRemaining;
        this.MaxMaterials = _maxMaterials;
    }

}
