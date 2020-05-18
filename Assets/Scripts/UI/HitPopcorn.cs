using UnityEngine;
using UnityEngine.UI;

public class HitPopcorn : MonoBehaviour
{
    private Text _textComponent;
    private RectTransform _rectTransform;
    private float _creationTime = 0;
    private float _duration = 1.5f;
    private Vector3 _offset;

    public Vector3 WorldPosition { get; set; }

    public Text Text
    {
        get
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponent<Text>();
            }

            return _textComponent;
        }
    }

    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }

    private void Start()
    {
        Destroy(gameObject, _duration);
        _creationTime = Time.time;
        _offset = new Vector2((Random.value - 0.5f) * 30.0f, (Random.value - 0.5f) * 30.0f);
    }

    internal void UpdatePosition(float timeAlive)
    {
        var screenCoordinate = Camera.main.WorldToScreenPoint(this.WorldPosition);

        // Make the popcorn scrol up slowly over time
        screenCoordinate += new Vector3(0, timeAlive*100, 0);
        screenCoordinate += _offset;

        this.RectTransform.anchoredPosition = screenCoordinate;
    }

    internal void UpdateAlpha(float timeAlive)
    {
        var color = this.Text.color;
        color.a = 1-(timeAlive / _duration);

        this.Text.color = color;
    }

    private void Update()
    {
        var timeAlive = Time.time - _creationTime;

        this.UpdatePosition(timeAlive);
        this.UpdateAlpha(timeAlive);
    }
}
