using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameExit : MonoBehaviour
{
    public void ReturnToMain(bool win)
    {
        StartCoroutine(ReturnRoutine(win));
        Debug.Log("[MiniGameExit] Returning to main scene..." + (win ? " (win)" : " (lose)"));
        
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
        Debug.Log("[MiniGameExit] Applied result to walker: " + (w != null ? w.name : "null"));
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

        Debug.Log("[MiniGameExit] Returned to main scene)))" + (win ? " (win)" : " (lose)"));

        MedZoneContext.Clear();

    }
}
