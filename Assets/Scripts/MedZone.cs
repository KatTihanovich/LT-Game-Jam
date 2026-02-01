using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class MedZone : MonoBehaviour
{
    // базовые префиксы, можно менять в инспекторе
    [Header("Scene name patterns")]
    public string infectedPrefix = "Health";   // для Infected
    public string sickPrefix  = "MakeMask";     // для Sick

    public bool onlyDragged  = true;
    public bool onlyInfected = false;            // теперь можем обрабатывать и Healthy

    private bool isLoading   = false;
    private bool sceneLoaded = false;

    private void Awake()
    {
        var box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CycleWalker walker = other.GetComponent<CycleWalker>();
        if (walker == null) return;

        if (onlyDragged && !walker.IsDragged)
            return;

        if (onlyInfected && walker.currentState == CycleWalker.State.Healthy)
            return;

        string targetScene = GetTargetSceneName(walker);
        if (string.IsNullOrEmpty(targetScene))
            return;

        // Сохраняем, кто именно зашёл в медзону
        MedZoneContext.CurrentWalker = walker;

        if (!sceneLoaded && !isLoading)
        {
            StartCoroutine(SwitchToScene(targetScene));
        }
    }

    private string GetTargetSceneName(CycleWalker walker)
    {
        string id = walker.idLetter; // буква из инспектора у персонажа

        switch (walker.currentState)
        {
            case CycleWalker.State.Infected:
                return infectedPrefix + id;   // пример: "MakeMaskA"
            case CycleWalker.State.Sick:
                return sickPrefix + id;    // пример: "HealthA"
            case CycleWalker.State.Healthy:
                return null;
            default:
                return null;
        }
    }

    private IEnumerator SwitchToScene(string sceneName)
    {
        isLoading = true;

        Scene previousActive = SceneManager.GetActiveScene();

        // Save return info
        MedZoneContext.PreviousScene = previousActive;
        MedZoneContext.DisabledRoots.Clear();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (!newScene.IsValid())
        {
            Debug.LogError("[MedZone] Не удалось получить сцену " + sceneName);
            isLoading = false;
            yield break;
        }

        SceneManager.SetActiveScene(newScene);

        var roots = previousActive.GetRootGameObjects();
        foreach (var go in roots)
        {
            go.SetActive(false);
            MedZoneContext.DisabledRoots.Add(go);
        }

        sceneLoaded = true;
        isLoading = false;
    }

}
