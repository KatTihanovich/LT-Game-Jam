using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameExit : MonoBehaviour
{
    public void ReturnToMain(bool win)
    {
        StartCoroutine(ReturnRoutine(win));
    }

    private IEnumerator ReturnRoutine(bool win)
    {
        // Apply result BEFORE returning
        if (win)
        {
            var w = MedZoneContext.CurrentWalker;
            if (w != null)
                w.Heal(); // or w.CureToHealthy();
        }

        // Reactivate previous scene objects
        foreach (var go in MedZoneContext.DisabledRoots)
            if (go != null) go.SetActive(true);

        // Set active scene back
        if (MedZoneContext.PreviousScene.IsValid())
            SceneManager.SetActiveScene(MedZoneContext.PreviousScene);

        // Unload this mini-game scene (the scene this script is in)
        Scene mini = gameObject.scene;
        AsyncOperation unload = SceneManager.UnloadSceneAsync(mini);
        while (unload != null && !unload.isDone)
            yield return null;

        MedZoneContext.Clear();
    }
}
