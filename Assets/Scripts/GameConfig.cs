using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{ 
    [SerializeField] private Structure[] _structures;

    public static GameConfig Instance { get; private set; }

    public Structure[] Structures => Instance._structures;

    void Awake()
    {
        Instance = this;
    }
}
