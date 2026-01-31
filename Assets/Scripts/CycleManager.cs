using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleManager : MonoBehaviour
{
    public static CycleManager Instance;

    public Transform sourcePoint;
    public GameObject fadeImage;
    public Source source;   // <--- повесь это поле в инспекторе

    private List<CycleWalker> walkers = new List<CycleWalker>();
    private int finishedCount = 0;
    private int reachedSourceCount = 0;

    private void Awake()
    {
        Instance = this;
        Debug.Log(sourcePoint.position);
    }

    public void Register(CycleWalker walker)
    {
        if (!walkers.Contains(walker))
            walkers.Add(walker);
    }

    public void Unregister(CycleWalker walker)
    {
        if (walkers.Contains(walker))
            walkers.Remove(walker);
    }

    // Закончил маршрут
    public void NotifyFinished()
    {
        finishedCount++;
        if (finishedCount >= walkers.Count && walkers.Count > 0)
        {
            StartCoroutine(SendAllToSource());
        }
    }

    public void NotifyReachedSource()
    {
        reachedSourceCount++;
        if (reachedSourceCount >= walkers.Count-2 && walkers.Count > 0)
        {
            StartCoroutine(NightRoutine());
        }
    }

    private IEnumerator SendAllToSource()
    {
        foreach (var w in walkers)
        {
            if (w != null);
                
        }

        yield return null;
    }

    private IEnumerator NightRoutine()
    {
        fadeImage.SetActive(true);
        yield return new WaitForSeconds(3f);

        // НОЧНАЯ ФАЗА: заражение и изменение HP источника
        if (source != null)
        {
            Debug.Log("[CycleManager] Starting EndOfDay on Source");
        }
        else
        {
            Debug.LogWarning("[CycleManager] Source не назначен в инспекторе!");
        }

        // Готовим новый день
        finishedCount = 0;
        reachedSourceCount = 0;

        foreach (var w in walkers)
        {
            if (w != null)
                w.StartNewCycle();
        }

        fadeImage.SetActive(false);
    }
}
