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
        var w = MedZoneContext.CurrentWalker;

        // Apply result BEFORE returning
        if (win)
        {
            if (w != null)
                w.Heal(); // or w.CureToHealthy();
        }
        w.enabled = true;

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

        Debug.Log("[MiniGameExit] Returned to main scene" + (win ? " (win)" : " (lose)"));

        var medZone = Object.FindFirstObjectByType<MedZone>();
        Debug.Log("Finding MedZone to reset state...(found: " + (medZone != null) + ")");
            if (medZone != null)
                medZone.ResetState();

        MedZoneContext.Clear();
    }
}
