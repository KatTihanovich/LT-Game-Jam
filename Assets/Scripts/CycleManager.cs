using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CycleManager : MonoBehaviour
{
    public static CycleManager Instance;

    public Transform sourcePoint; // Источник

    public GameObject fadeImage; // Чёрный UI Image

    private List<CycleWalker> walkers = new List<CycleWalker>();

    private int finishedCount = 0;

    void Awake()
    {
        Instance = this;
    }

    public void Register(CycleWalker walker)
    {
        walkers.Add(walker);
    }

    public void NotifyFinished()
    {
        finishedCount++;

        if (finishedCount >= walkers.Count)
        {
            StartCoroutine(EndCycle());
        }
    }

    IEnumerator EndCycle()
    {
        // Все идут к источнику
        foreach (var w in walkers)
        {
            w.GoToSource();
        }

        yield return new WaitForSeconds(3f);

        fadeImage.SetActive(true);
        finishedCount = 0;

        foreach (var w in walkers)
        {
            w.StartNewCycle();
        }

        fadeImage.SetActive(false);
    }
}
