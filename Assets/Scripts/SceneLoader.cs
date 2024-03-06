using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Scenes sceneToLoad;

    private Coroutine coroutine;

    public SceneLoader(Scenes scene)
    {
        sceneToLoad = scene;
    }

    private IEnumerator Start()
    {
        coroutine = StartCoroutine(LoadSceneAsync());
        yield return null;
    }

    public IEnumerator LoadSceneAsync()
    {
        var asyncOp = SceneManager.LoadSceneAsync((int)sceneToLoad);

        while (!asyncOp.isDone) yield return null;
    }
}

public enum Scenes
{
    MenuScene = 1,
    BattlefieldScene = 2
}