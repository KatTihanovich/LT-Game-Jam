using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHider
{
    public static void HideActiveScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!active.IsValid()) return;

        var roots = active.GetRootGameObjects();   // все корневые объекты текущей сцены [web:57][web:58]
        foreach (var go in roots)
        {
            go.SetActive(false);                  // сцена «невидима», но остаётся загруженной
        }
    }

    public static void ShowActiveScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!active.IsValid()) return;

        var roots = active.GetRootGameObjects();
        foreach (var go in roots)
        {
            go.SetActive(true);
        }
    }
}
