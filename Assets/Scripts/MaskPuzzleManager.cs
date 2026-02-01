using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MaskPuzzleManager : MonoBehaviour
{
    [Header("Puzzle parts")]
    public DraggablePieceUI[] pieces;

    [Header("UI to toggle")]
    public GameObject piecesRoot;    
    public GameObject outlineObject;  
    public GameObject fullMaskObject; 

    public ExposureByResult_URP exposure;

    private bool completed;

    [Header("Next scene")]
    public string nextSceneName;      
    public float delayBeforeNextScene = 2f;

    public void CheckComplete()
    {
        if (completed) return;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (!pieces[i].IsLocked)
                return;
        }

        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        completed = true;
        exposure.OnWin(); 

        if (piecesRoot) piecesRoot.SetActive(false);
        if (outlineObject) outlineObject.SetActive(false);
        if (fullMaskObject) fullMaskObject.SetActive(true);

        Debug.Log("Mask completed!");
        if (!string.IsNullOrEmpty(nextSceneName))
            StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeNextScene);
        SceneManager.LoadScene(nextSceneName);
    }
}
