internal class PlayerDeathEvent : GameEvent
{
    public PlayerDeathEvent(object serializedData) : this((TeamColor) ((object[])serializedData)[0])
    {
    }

    public PlayerDeathEvent(TeamColor teamColor)
    {
        this.PlayerTeam = teamColor;
    }

    public PlayerDeathEvent(PlayerController controller)
    {
        this.PlayerTeam = controller.TeamId;
    }

    internal override object[] Serialize()
        => new object[] { (byte)this.PlayerTeam };

    public TeamColor PlayerTeam { get; private set; }
}
