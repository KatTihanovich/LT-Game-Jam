using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MedZoneContext
{
    public static CycleWalker CurrentWalker;

    // to return back
    public static Scene PreviousScene;
    public static List<GameObject> DisabledRoots = new List<GameObject>();

    public static void Clear()
    {
        CurrentWalker = null;
        DisabledRoots.Clear();
        PreviousScene = default;
    }
}
