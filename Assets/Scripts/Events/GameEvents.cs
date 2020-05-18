using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

internal class GameEvents
{
    private static byte sNextEventCode = 0;

    private static Dictionary<Type, byte> sEventCodes = new Dictionary<Type, byte>();

    private static Type[] sRegisteredEvents = new Type[]{
        typeof(FlagStatusChangedEvent),
        typeof(PlayerDeathEvent),
        typeof(PlayerRespawnEvent)
    };

    static GameEvents()
    {
        for (var i = 0; i < sRegisteredEvents.Length; i++)
        {
            sEventCodes[sRegisteredEvents[i]] = (byte)(i + 1);
        }
    }

    internal static byte GetEventCode(Type type)
    {
        if (!sEventCodes.TryGetValue(type, out var eventCode))
        {
            eventCode = (byte)(sEventCodes.Count + 1);
            sEventCodes.Add(type, eventCode);
        }

        return eventCode;
    }

    internal static void FireEvent(GameEvent evt)
        => FireEvent(evt, RaiseEventOptions.Default);

    internal static void FireEvent(GameEvent evt, RaiseEventOptions options)
    {
        options = options ?? RaiseEventOptions.Default;

        PhotonNetwork.RaiseEvent(GetEventCode(evt.GetType()), evt.Serialize(), options, ExitGames.Client.Photon.SendOptions.SendReliable);
    }
}