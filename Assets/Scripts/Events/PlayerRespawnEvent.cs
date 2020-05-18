internal class PlayerRespawnEvent : GameEvent
{
    public PlayerRespawnEvent(object serializedData) : this((TeamColor)((object[])serializedData)[0])
    {
    }

    public PlayerRespawnEvent(TeamColor teamColor)
    {
        this.PlayerTeam = teamColor;
    }

    public PlayerRespawnEvent(PlayerController controller)
    {
        this.PlayerTeam = controller.TeamId;
    }

    internal override object[] Serialize()
        => new object[] { (byte)this.PlayerTeam };

    public TeamColor PlayerTeam { get; private set; }
}
