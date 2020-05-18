using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiableObject : MonoBehaviour
{
    [SerializeField] private string _objectIdentifier = "";
    [SerializeField] private bool _startActive = false;

    private static Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();

    public static bool TryIdentify(string name, out GameObject obj)
        => _objects.TryGetValue(name, out obj);

    private void Awake()
    {
        if (string.IsNullOrEmpty(_objectIdentifier))
        {
            return;
        }

        _objects[_objectIdentifier] = gameObject;
    }

    private void Start()
    {
        gameObject.SetActive(_startActive);
    }
}
