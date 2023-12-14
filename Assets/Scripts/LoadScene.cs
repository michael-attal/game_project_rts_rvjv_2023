using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private int sceneIndex;

    private Coroutine coroutine;

    private IEnumerator Start()
    {
        coroutine = StartCoroutine(LoadSceneAsync());
        yield return null;
    }

    private IEnumerator LoadSceneAsync()
    {
        var asyncOp = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncOp.isDone) yield return null;
    }
}