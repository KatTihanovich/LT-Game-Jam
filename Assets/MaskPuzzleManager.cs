using UnityEngine;

public class MaskPuzzleManager : MonoBehaviour
{
    [Header("Puzzle parts")]
    public DraggablePieceUI[] pieces;

    [Header("UI to toggle")]
    public GameObject piecesRoot;     // parent of all pieces (tray + placed ones)
    public GameObject outlineObject;  // your big outline image object
    public GameObject fullMaskObject; // completed mask image object (disabled at start)

    private bool completed;

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

        if (piecesRoot) piecesRoot.SetActive(false);
        if (outlineObject) outlineObject.SetActive(false);
        if (fullMaskObject) fullMaskObject.SetActive(true);

        // optional: play sound / particles here
        Debug.Log("Mask completed!");
    }
}
