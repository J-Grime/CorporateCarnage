using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Flag : MonoBehaviour, IOnEventCallback
{
    public static Flag Instance { get; private set; }

    private Collider[] _colliders;
    private Renderer[] _renderers;
    public TeamColor holderTM;
    public bool leftZone = true;

    void Start()
    {
        Instance = this;

        _colliders = GetComponentsInChildren<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();

        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void SetFlagStatus(bool active, Vector3 position)
    {
        GameEvents.FireEvent(new FlagStatusChangedEvent(active, position), new Photon.Realtime.RaiseEventOptions()
        {
            Receivers = Photon.Realtime.ReceiverGroup.All
        });

        this.SetFlagStatusInternal(active, position);
    }

    private void SetFlagStatusInternal(bool active, Vector3 position)
    {
        foreach(var renderer in _renderers)
            renderer.enabled = active;
        foreach(var collider in _colliders)
            collider.enabled = active;

        gameObject.transform.position = position;
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != GameEvents.GetEventCode(typeof(FlagStatusChangedEvent)))
            return;

        var eventData = new FlagStatusChangedEvent(photonEvent.CustomData);
        this.SetFlagStatusInternal(eventData.IsFlagActive, eventData.FlagPosition);
    }
}
