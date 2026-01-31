using System.Collections.Generic;
using UnityEngine;

public class CycleWalker : MonoBehaviour
{
    public enum State { Healthy, Infected, Sick }

    public float speed = 2f;

    private float minX, maxX, minY, maxY;
    private List<Vector2> points = new List<Vector2>();
    private int currentIndex = 0;
    private Vector2 target;

    private bool isInitialized = false;

    private bool inQuarantine = false;
    public bool InQuarantine => inQuarantine;

    // ========== ИНФЕКЦИЯ ==========
    public State currentState = State.Healthy;

    // Порог по числу посещений источника для перехода в Infected
    private int sourceVisitCount = 0;
    private int infectionThresholdVisits = 0;

    // Индекс точки, на которой Infected станет Sick
    private int infectedThresholdIndex = -1;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateColor();

        // Случайный порог визитов к источнику: 5–10 включительно
        infectionThresholdVisits = Random.Range(5, 11);
        Debug.Log($"[Walker {name}] infectionThresholdVisits = {infectionThresholdVisits}");
    }

    void Update()
    {
        if (!isInitialized) return;
        if (inQuarantine) return;
        Move();
    }

    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target) < 0.05f)
            OnReachPoint();
    }

    void OnReachPoint()
    {
        // Проверяем, является ли текущая точка источником
        if (Vector2.Distance(points[currentIndex], Source.Instance.transform.position) < 0.01f)
        {
            // Сообщаем источнику о посещении
            Source.Instance?.OnWalkerVisitedSource(this);

            // Считаем визиты к источнику
            sourceVisitCount++;

            // Когда превысили порог — становимся Infected
            if (currentState == State.Healthy && currentIndex >= infectionThresholdVisits)
            {
                Infect();
                Debug.Log($"[Walker {name}] заразился после {sourceVisitCount} визитов к источнику");
            }
        }

        // Проверяем переход Infected → Sick по индексу точки
        Sick();

        currentIndex++;
        if (currentIndex >= points.Count)
        {
            // Новый цикл из 10 точек (1 точка — источник)
            StartNewCycle();
            return;
        }

        target = points[currentIndex];
    }

    // ========== ЦИКЛ ==========
    public void StartNewCycle()
    {
        points.Clear();
        currentIndex = 0;

        GeneratePoints();

        if (!inQuarantine && points.Count > 0)
            target = points[0];
    }

    void GeneratePoints()
    {
        // 9 случайных точек
        for (int i = 0; i < 9; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            points.Add(new Vector2(x, y));
        }

        // 1 точка — источник, случайная позиция в списке 1..9 (итого 10 точек)
        int index = Random.Range(1, 10);
        Vector2 sourcePos = Source.Instance.transform.position;
        points.Insert(index, sourcePos);
    }

    // ========== КАРАНТИН ==========
    public void EnterQuarantine()
    {
        inQuarantine = true;
        target = transform.position;
    }

    public void ExitQuarantine()
    {
        inQuarantine = false;
        if (points.Count > 0 && currentIndex < points.Count)
            target = points[currentIndex];
    }

    // ========== ЛЕЧЕНИЕ КЛИКОМ ==========
    void OnMouseDown()
    {
        if (!InQuarantine) return;
        if (currentState == State.Healthy) return;

        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        foreach (var h in hits)
        {
            QuarantineZone zone = h.GetComponent<QuarantineZone>();
            if (zone != null)
            {
                zone.HealWalker(this);
                return;
            }
        }
    }

    // ========== ПРОГРЕСС БОЛЕЗНИ ==========
    // Infected → Sick по достижении infectedThresholdIndex
    public void Sick()
    {
        if (currentState == State.Infected &&
            infectedThresholdIndex >= 0 &&
            currentIndex == infectedThresholdIndex)
        {
            currentState = State.Sick;
            UpdateColor();
            Debug.Log($"[Walker {name}] стал Sick на точке {currentIndex}");
        }
    }

    public void Infect()
    {
        if (currentState == State.Healthy)
        {
            currentState = State.Infected;
            UpdateColor();

            // В момент заражения выбираем точку маршрута, на которой он станет Sick.
            // Берём одну из следующих точек текущего цикла.
            if (points.Count > currentIndex + 1)
            {
                infectedThresholdIndex = Random.Range(currentIndex + 1, points.Count);
            }
            else
            {
                // на всякий случай — если заражение случилось в самом конце цикла
                infectedThresholdIndex = -1;
            }

            Debug.Log($"[Walker {name}] станет Sick на точке {infectedThresholdIndex}");
        }
    }

    public void Heal()
    {
        currentState = State.Healthy;
        UpdateColor();

        // После лечения задаём новый порог визитов и сбрасываем прогресс
        infectionThresholdVisits = Random.Range(5, 11);
        sourceVisitCount = 0;
        infectedThresholdIndex = -1;
        Debug.Log($"[Walker {name}] новый infectionThresholdVisits = {infectionThresholdVisits}");
        infectionThresholdVisits = Random.Range(5, 11);

    }

    private void UpdateColor()
    {
        if (sr == null) return;

        switch (currentState)
        {
            case State.Healthy:  sr.color = Color.green;  break;
            case State.Infected: sr.color = Color.yellow; break;
            case State.Sick:     sr.color = Color.red;    break;
        }
    }

    // ========== ИНИЦИАЛИЗАЦИЯ ==========
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;

        isInitialized = true;
        StartNewCycle();
    }
}
