using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private static HealthBar sInstance;

    [SerializeField] private RectTransform _greenMaskTransform;
    [SerializeField] private RectTransform _greenTransform;

    [SerializeField] private RectTransform _blackMaskTransform;
    [SerializeField] private RectTransform _blackTransform;

    [SerializeField] private Image _maskImage;
    
    private float _maskStartWidth = 0;
    private int height = 233;

    private int maskStartX;
    private int imgStartX;

    public static HealthBar Instance
        => sInstance;

    internal void Awake()
    {
        if (_greenMaskTransform == null)
        {
            _greenMaskTransform = GetComponentInChildren<UnityEngine.UI.Mask>().gameObject.GetComponent<RectTransform>();
            Debug.Log("Health bar does not have a mask transform. We've found one automatically.");
        }

        imgStartX = (int)_greenTransform.transform.localPosition.x;
        maskStartX = (int)_greenMaskTransform.transform.localPosition.x;
        
        sInstance = this;
    }

    void Update()
    {
        if (PlayerController.LocalPlayerController == null) {
            return;
        }

        var health = PlayerController.LocalPlayerController.Health;
        var healthRemainingPercent = (health / 100.0f);
        var maskOffsetX = height * (1.0 - healthRemainingPercent);

        _greenMaskTransform.localPosition = new Vector3((float)(maskStartX - maskOffsetX), _greenMaskTransform.localPosition.y, _greenMaskTransform.localPosition.y);
        _greenTransform.localPosition = new Vector3((float)(imgStartX + maskOffsetX), _greenTransform.localPosition.y, _greenTransform.localPosition.y);
        
        _blackMaskTransform.localPosition = new Vector3((float)(maskStartX - maskOffsetX + (maskOffsetX >5? 5: 0)), _blackMaskTransform.localPosition.y, _blackMaskTransform.localPosition.y);
        _blackTransform.localPosition = new Vector3((float)(imgStartX + maskOffsetX + (maskOffsetX > 5 ? 5 : 0)), _blackTransform.localPosition.y, _blackTransform.localPosition.y);
        

        //_maskTransform.sizeDelta = new Vector2(_maskStartWidth * (PlayerController.LocalPlayerController.Health / 100.0f), _maskTransform.sizeDelta.y);
    }
}
