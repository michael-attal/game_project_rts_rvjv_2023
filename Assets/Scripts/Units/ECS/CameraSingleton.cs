using Unity.Entities;
using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    public static Camera Instance;

    private void Awake()
    {
        Instance = GetComponent<Camera>();
        Debug.Log("CameSingleton created!");
    }
}

public struct ICCamera : IComponentData
{
}