using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum GameState : byte
{
    None,
    Menu,
    PreRound,
    Building,
    Playing,
}

[RequireComponent(typeof(PhotonView))]
public class Gamemode : MonoBehaviourPun
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _playerSpawnpoints;

    [SerializeField] private float _roundBuildingLength = 5;
    [SerializeField] private float _roundPlayLength = 8;

    private PlayerSpawn[] _playerSpawns;
    private ScoringZone[] _scoringZones;

    private GameState _gameState = GameState.None;
    private float _phaseEndTime = 2;
    private float _lastSynchronizeTime = 0;
    private static Gamemode _instance;

    public delegate void GameStateChanged(GameState oldGameState, GameState newGameState);
    public event GameStateChanged OnGameStateChanged;

    private PhotonView _photonView;

    internal List<PlayerController> PlayerControllers { get; } = new List<PlayerController>();

    internal float PhaseTimeRemaining
        => _phaseEndTime - Time.time;


    internal GameState CurrentGameState
    {
        get => _gameState;
        set
        {
            if (_gameState == value) return;

            var oldGameState = _gameState;
            _gameState = value;

            switch (value)
            {
                case GameState.Building:
                    EnterBuildingMode();
                    break;
                case GameState.Playing:
                    EnterPlayingMode();
                    break;
            }

            OnGameStateChanged?.Invoke(oldGameState, value);
        }
    }

    internal static Gamemode Instance
        => _instance;

    private void Awake()
        => _instance = this;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();

        var spawns = FindObjectsOfType<PlayerSpawn>();

        _playerSpawns = new PlayerSpawn[]
        {
            spawns.FirstOrDefault(spawn => spawn.TeamColor == TeamColor.Red),
            spawns.FirstOrDefault(spawn => spawn.TeamColor == TeamColor.Green),
            spawns.FirstOrDefault(spawn => spawn.TeamColor == TeamColor.Yellow),
            spawns.FirstOrDefault(spawn => spawn.TeamColor == TeamColor.Blue),
        };

        foreach (var spawn in spawns)
        {
            spawn.Spawn();
        }

        _scoringZones = FindObjectsOfType<ScoringZone>();

        this.CurrentGameState = GameState.PreRound;
    }

    void EnterBuildingMode()
    {
        this.SetObjectEnabled("IngamePlayerStats", false);
        this.SetObjectEnabled("MaterialsRemainingUI", true);
        this.SetObjectEnabled("BuildingHotbar", true);

        this.SetObjectEnabled("RedScoringZone", false);

        this.SetObjectEnabled("RedGrid", true);
        this.SetObjectEnabled("GreenGrid", true);
        this.SetObjectEnabled("YellowGrid", true);
        this.SetObjectEnabled("BlueGrid", true);

        // Make scoring zones hidden
        foreach (var scoringZone in _scoringZones)
        {
            scoringZone.gameObject.SetActive(false);
        }

        foreach (var player in this.PlayerControllers.Where(ply => ply != null))
        {
            // Send all players back to their spawn points
            player.Respawn();
        }

        // 30 seconds to build
        _phaseEndTime = Time.time + _roundBuildingLength;
    }

    void EnterPlayingMode()
    {
        this.SetObjectEnabled("IngamePlayerStats", true);;
        this.SetObjectEnabled("MaterialsRemainingUI", false);
        this.SetObjectEnabled("BuildingHotbar", false);

        FindObjectOfType<CameraBuild>().AbortStructure();

        foreach (var scoringZone in _scoringZones)
        {
            scoringZone.gameObject.SetActive(true);
        }

        this.SetObjectEnabled("RedGrid", false);
        this.SetObjectEnabled("GreenGrid", false);
        this.SetObjectEnabled("YellowGrid", false);
        this.SetObjectEnabled("BlueGrid", false);

        _phaseEndTime = Time.time + _roundPlayLength;
    }

    void Update()
    {
        this.SynchronizeTimeRemaining();
        var timeRemaining = this.PhaseTimeRemaining;

        if (timeRemaining <= 0)
        {
            switch (_gameState)
            {
                case GameState.Building:
                    this.CurrentGameState = GameState.Playing;
                    break;
                case GameState.Menu:
                    break;
                case GameState.PreRound:
                case GameState.Playing:
                    this.CurrentGameState = GameState.Building;
                    break;
            }
        }
    }

    private void SynchronizeTimeRemaining()
    {
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.IsMasterClient) return;

        if (this.PhaseTimeRemaining > 2 && Time.time - _lastSynchronizeTime > 0.5)
        {
            return;
        }

        _lastSynchronizeTime = Time.time;

        var photonView = PhotonView.Get(this);
        photonView.RPC("SynchronizeTimeRemainingRPC", RpcTarget.Others, (byte) this.CurrentGameState, _phaseEndTime, Time.time);
    }

    [PunRPC]
    public void SynchronizeTimeRemainingRPC(byte gameState, float endTime, float hostCurrentTime)
    {
        this.CurrentGameState = (GameState) gameState;

        // Add the difference in host times from the end time to account for latency
        _phaseEndTime = endTime + (Time.time - hostCurrentTime);
    }

    GameObject SetObjectEnabled(string name, bool enabled)
    {
        if (IdentifiableObject.TryIdentify(name, out var obj))
            obj.gameObject.SetActive(enabled);

        return obj;
    }
}
