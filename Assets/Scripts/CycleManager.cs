using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleManager : MonoBehaviour
{
    public static CycleManager Instance;

    public Transform sourcePoint;
    public GameObject fadeImage;

    private List<CycleWalker> walkers = new List<CycleWalker>();

    private int finishedCount = 0;
    private int reachedSourceCount = 0;

    void Awake()
    {
        Instance = this;
    }

    public void Register(CycleWalker walker)
    {
        walkers.Add(walker);
    }

    // Закончил маршрут
    public void NotifyFinished()
    {
        Debug.Log("Walker finished cycle");
        finishedCount++;

        if (finishedCount >= walkers.Count)
        {
            StartCoroutine(SendAllToSource());
        }
    }

    // Дошёл до источника
    public void NotifyReachedSource()
    {
        reachedSourceCount++;

        if (reachedSourceCount >= walkers.Count)
        {
            Debug.Log("All reached source, starting night routine");
            StartCoroutine(NightRoutine());
        }
    }

    IEnumerator SendAllToSource()
    {
        foreach (var w in walkers)
        {
            Debug.Log("Sending to source");
            w.GoToSource();
        }

        yield return null;
    }

    IEnumerator NightRoutine()
    {
        Debug.Log("Starting night routine");
        fadeImage.SetActive(true);
        yield return new WaitForSeconds(3f);


        finishedCount = 0;
        reachedSourceCount = 0;

        foreach (var w in walkers)
        {
            w.StartNewCycle();
        }

        fadeImage.SetActive(false);
    }
}
