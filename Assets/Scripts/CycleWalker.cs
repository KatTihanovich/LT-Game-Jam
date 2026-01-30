using System.Collections.Generic;
using UnityEngine;

public class CycleWalker : MonoBehaviour
{
    public float speed = 2f;

    public float minX, maxX, minY, maxY;

    private List<Vector2> points = new List<Vector2>();

    private int currentIndex = 0;

    private Vector2 target;

    private bool goingToSource = false;
    private bool cycleFinished = false;

    void Start()
    {
        CycleManager.Instance.Register(this);

        StartNewCycle();
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            OnReachPoint();
        }
    }

    void OnReachPoint()
    {
        if (goingToSource)
            return;

        currentIndex++;

        if (currentIndex >= points.Count)
        {
            if (!cycleFinished)
            {
                cycleFinished = true;
                CycleManager.Instance.NotifyFinished();
            }

            return;
        }

        target = points[currentIndex];
    }

    public void StartNewCycle()
    {
        points.Clear();
        currentIndex = 0;
        cycleFinished = false;
        goingToSource = false;

        GeneratePoints();

        target = points[0];
    }

    void GeneratePoints()
    {
        // Генерируем 9 случайных
        for (int i = 0; i < 9; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);

            points.Add(new Vector2(x, y));
        }

        // Вставляем источник в случайное место
        int index = Random.Range(0, 10);

        Vector2 sourcePos =
            CycleManager.Instance.sourcePoint.position;

        points.Insert(index, sourcePos);
    }

    public void GoToSource()
    {
        goingToSource = true;

        target = CycleManager.Instance.sourcePoint.position;
    }
}
