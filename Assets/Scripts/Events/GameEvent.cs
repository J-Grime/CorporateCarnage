internal abstract class GameEvent
{
    internal byte EventCode { get; set; }

    internal abstract object[] Serialize();
}