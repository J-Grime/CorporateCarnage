using UnityEngine;

internal class FlagStatusChangedEvent : GameEvent
{
    public FlagStatusChangedEvent(object serializedData) : this((bool)((object[])serializedData)[0],
                                                                (Vector3)((object[])serializedData)[1])
    {
    }

    public FlagStatusChangedEvent(bool isFlagActive, Vector3 flagPosition)
    {
        this.IsFlagActive = isFlagActive;
        this.FlagPosition = flagPosition;
    }

    public bool IsFlagActive { get; private set; }

    public Vector3 FlagPosition { get; private set; }

    internal override object[] Serialize()
        => new object[] { this.IsFlagActive, this.FlagPosition };
}