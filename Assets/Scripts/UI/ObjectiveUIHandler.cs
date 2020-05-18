using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUIHandler : MonoBehaviour
{
    [SerializeField] private Sprite _redObjectiveSprite;
    [SerializeField] private Sprite _greenObjectiveSprite;
    [SerializeField] private Sprite _blueObjectiveSprite;
    [SerializeField] private Sprite _yellowObjectiveSprite;
    [SerializeField] private Sprite _flagObjectiveSprite;

    [SerializeField] private Image _objectiveImage;

    private Dictionary<TeamColor, Sprite> _spriteTeamMap;

    private void Awake()
    {
        _spriteTeamMap = new Dictionary<TeamColor, Sprite>();
        _spriteTeamMap.Add(TeamColor.Red, _redObjectiveSprite);
        _spriteTeamMap.Add(TeamColor.Green, _greenObjectiveSprite);
        _spriteTeamMap.Add(TeamColor.Blue, _blueObjectiveSprite);
        _spriteTeamMap.Add(TeamColor.Yellow, _yellowObjectiveSprite);
    }

    private void Update()
    {
        var isFlagVisible = this.UpdateIconPosition();
        var alpha = _objectiveImage.color.a;

        // Update alpha depending on whether we can currently see the flag
        alpha = isFlagVisible ? Mathf.Min(alpha + Time.deltaTime, 1.0f) : Mathf.Max(alpha - Time.deltaTime, 0);

        _objectiveImage.color = new Color(_objectiveImage.color.r, _objectiveImage.color.g, _objectiveImage.color.b, alpha);
    }

    private bool UpdateIconPosition()
    {
        if (Gamemode.Instance == null || PlayerController.LocalPlayerController == null) return false;

        var playerWithFlag = this.FindPlayerHoldingFlag();
        var localPlayer = PlayerController.LocalPlayerController;

        if (playerWithFlag == localPlayer) return false;

        Sprite spriteToUse;
        Vector3 flagPosition;

        var raycastFrom = Camera.main.transform.position + (Camera.main.transform.forward * 3);

        if (playerWithFlag != null)
        {
            spriteToUse = _spriteTeamMap[playerWithFlag.TeamId];
            flagPosition = playerWithFlag.gameObject.transform.position;
        }
        else
        {
            spriteToUse = _flagObjectiveSprite;
            flagPosition = Flag.Instance.transform.position;
        }

        var canSee = true;
        if (Physics.Linecast(raycastFrom, flagPosition + new Vector3(0, 0.5f, 0), out var hit))
        {
            if (Vector3.Distance(hit.point, flagPosition) < 3)
            {
                // We can see the flag
                canSee = false;
            }
        }

        var distance = Vector3.Distance(localPlayer.transform.position, flagPosition);

        if (_objectiveImage.sprite != spriteToUse)
            _objectiveImage.sprite = spriteToUse;

        var cameraTransform = Camera.main.transform;
        var positionDiff = (cameraTransform.position - flagPosition).normalized;
        var facingDirection = cameraTransform.forward;

        var screenPosition = Camera.main.WorldToScreenPoint(flagPosition);

        if (screenPosition.z < 0)
        {
            screenPosition.x = Screen.width * -Mathf.Sign(screenPosition.x);
        }

        screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
        screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);

        _objectiveImage.transform.position = screenPosition;

        return canSee;
    }

    private PlayerController FindPlayerHoldingFlag()
        => Gamemode.Instance.PlayerControllers.FirstOrDefault(player => player.IsHoldingFlag);
}
