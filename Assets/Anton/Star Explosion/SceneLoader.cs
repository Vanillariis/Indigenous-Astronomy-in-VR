using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public string initialScene = "Desert- Anton";
    public FadeController fadeController;

    private string currentScene;

    void Start()
    {
        StartCoroutine(InitialLoad());
    }

    IEnumerator InitialLoad()
    {
        yield return fadeController.FadeOut();
        yield return LoadSceneAsync(initialScene);
        yield return fadeController.FadeIn();
    }

    public void TransitionTo(string targetScene)
    {
        StartCoroutine(Transition(targetScene));
    }

    IEnumerator Transition(string targetScene)
    {
        yield return fadeController.FadeOut();
        yield return UnloadSceneAsync(currentScene);
        yield return LoadSceneAsync(targetScene);
        yield return fadeController.FadeIn();
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        currentScene = sceneName;
    }

    IEnumerator UnloadSceneAsync(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
            while (!op.isDone)
                yield return null;
        }
    }
}