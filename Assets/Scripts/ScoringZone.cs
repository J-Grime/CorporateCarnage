using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringZone : MonoBehaviour
{
    [SerializeField] private TeamColor _scoringTeam;

    public TeamColor ScoringTeam
        => _scoringTeam;
}
