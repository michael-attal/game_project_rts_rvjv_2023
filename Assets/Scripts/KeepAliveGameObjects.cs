using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepAliveGameObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> gameObjectsToKeep;
    
    private void Awake()
    {
        foreach (var gameobject in gameObjectsToKeep)
        {
            DontDestroyOnLoad(gameobject);
        }
    }
}
