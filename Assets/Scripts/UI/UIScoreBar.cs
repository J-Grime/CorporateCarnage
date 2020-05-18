using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreBar : MonoBehaviour
{
    public static float SCORE_TO_WIN = 80;

    [SerializeField] private TeamColor _teamColor;
    [SerializeField] private int _fillWidth = 269;
    [SerializeField] private RectTransform _fillColorTransform;
    [SerializeField] private RectTransform _emptyColorTransform;
    [SerializeField] private Text _playerNameText;

    private float _lastScore;
    private RectTransform _rectTransform;

    private bool _isDisabled = false;

    public int ScoreRanking { get; set; }

    public bool IsDisabled
        => _isDisabled;

    public float Score
        => _lastScore;

    public float SpacingStride { get; set; }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Gamemode.Instance == null) return;

        var playerController = Gamemode.Instance.PlayerControllers.FirstOrDefault(ply => ply.TeamId == _teamColor);

        if (playerController == null)
        {
            if (_isDisabled) return;
            _isDisabled = true;

            foreach (var childTransform in GetComponentsInChildren<Transform>(true))
            {
                if (childTransform != transform)
                    childTransform.gameObject.SetActive(false);
            }

            return;
        }

        if (_isDisabled)
        {
            // If we are disabled, but now know where the character is, re-enable child objects

            foreach (var childTransform in GetComponentsInChildren<Transform>(true))
            {
                if(childTransform != transform)
                    childTransform.gameObject.SetActive(true);
            }

            _isDisabled = false;
        }

        _playerNameText.text = playerController.NickName;

        _lastScore = playerController?.Score ?? 0;
        var scorePct = Mathf.Min(1.0f, (float)(_lastScore / SCORE_TO_WIN));

        _fillColorTransform.sizeDelta = new Vector2(scorePct * _fillWidth, _fillColorTransform.sizeDelta.y);
        _emptyColorTransform.sizeDelta = new Vector2((1 - scorePct) * _fillWidth, _fillColorTransform.sizeDelta.y);

        var desiredLocalY = -this.SpacingStride * this.ScoreRanking;
        var localPos = _rectTransform.localPosition;
        var velocity = Time.deltaTime * 100;

        if (desiredLocalY > localPos.y)
        {
            _rectTransform.localPosition = new Vector3(localPos.x, Mathf.Min(localPos.y + velocity, desiredLocalY), localPos.z);
        }
        else if (desiredLocalY < localPos.y)
        {
            _rectTransform.localPosition = new Vector3(localPos.x, Mathf.Max(localPos.y - velocity, desiredLocalY), localPos.z);
        }
    }
}
