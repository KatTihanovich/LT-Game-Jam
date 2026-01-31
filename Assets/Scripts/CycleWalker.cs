using System.Collections.Generic;
using UnityEngine;

public class CycleWalker : MonoBehaviour
{
    public enum State { Healthy = 1, Infected = 2, Sick = 3 }

    public float speed = 2f;

    private float minX, maxX, minY, maxY;
    private List<Vector2> points = new List<Vector2>();
    private int currentIndex = 0;
    private Vector2 target;

    private bool isInitialized = false;
    private bool inQuarantine = false;
    public bool InQuarantine => inQuarantine;

    // Тащится ли сейчас мышкой
    public bool IsDragged { get; private set; }

    public void SetDragged(bool value)
    {
        IsDragged = value;

        // Триггер поднятия
        if (value && animator != null)
        {
            animator.ResetTrigger("PulledUp");   // чтобы не залипало[web:24]
            animator.SetTrigger("PulledUp");     // запускаем анимацию поднятия[web:26]
        }
    }

    public State currentState = State.Healthy;

    private int sourceVisitCount = 0;
    private int infectionThresholdVisits = 0;
    private int infectedThresholdIndex = -1;

    private SpriteRenderer sr;
    private QuarantineZone quarantineZone;
    private Animator animator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        UpdateColor();
        UpdateAnimatorStage(); // сразу выставим Stage

        infectionThresholdVisits = Random.Range(5, 11);
        quarantineZone = FindObjectOfType<QuarantineZone>();
    }

    void Update()
    {
        if (!isInitialized) return;
        if (inQuarantine) return;

        Move();
    }

    void Move()
    {
        Vector2 currentPos = transform.position;

        // Вектор к цели
        Vector2 toTarget = target - currentPos;
        bool isMovingNow = toTarget.sqrMagnitude > 0.0001f;

        // Обновляем анимационные параметры движения
        if (animator != null)
        {
            Vector2 dir = isMovingNow ? toTarget.normalized : Vector2.zero;

            animator.SetBool("IsMoving", isMovingNow);
            animator.SetFloat("MoveX", -dir.x);              // SetFloat[web:3]
            animator.SetFloat("MoveY", dir.y);
            animator.SetBool("IsBack", dir.y > 0.01f);
        }

        // Само перемещение
        transform.position = Vector2.MoveTowards(
            currentPos,
            target,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target) < 0.05f)
            OnReachPoint();
    }

    void OnReachPoint()
    {
        if (Vector2.Distance(points[currentIndex], Source.Instance.transform.position) < 0.01f)
        {
            Source.Instance?.OnWalkerVisitedSource(this);
            sourceVisitCount++;

            if (currentState == State.Healthy && currentIndex >= infectionThresholdVisits)
            {
                Infect();
                Debug.Log($"[Walker {name}] заразился после {sourceVisitCount} визитов к источнику");
            }
        }

        Sick();

        currentIndex++;
        if (currentIndex >= points.Count)
        {
            StartNewCycle();
            return;
        }

        target = points[currentIndex];
    }

    // ====== ЦИКЛ ======

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
        for (int i = 0; i < 9; i++)
        {
            Vector2 point;
            int attempts = 0;

            do
            {
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);
                point = new Vector2(x, y);
                attempts++;

                if (attempts > 30)
                    break;

            } while (
                quarantineZone != null &&
                currentState != State.Healthy &&
                (
                    quarantineZone.ContainsPoint(point) ||
                    quarantineZone.IntersectsLine(transform.position, point)
                )
            );

            points.Add(point);
        }

        int index = Random.Range(1, points.Count + 1);
        points.Insert(index, Source.Instance.transform.position);
    }

    // ====== КАРАНТИН ======

    public void EnterQuarantine()
    {
        inQuarantine = true;
        target = transform.position;

        // Встал – значит не двигается
        if (animator != null)
            animator.SetBool("IsMoving", false);
    }

    public void ExitQuarantine()
    {
        inQuarantine = false;
        if (points.Count > 0 && currentIndex < points.Count)
            target = points[currentIndex];
    }

    // ====== БОЛЕЗНЬ ======

    public void Sick()
    {
        if (currentState == State.Infected &&
            infectedThresholdIndex >= 0 &&
            currentIndex == infectedThresholdIndex)
        {
            currentState = State.Sick;
            UpdateColor();
            UpdateAnimatorStage();
        }
    }

    public void Infect()
    {
        if (currentState != State.Healthy) return;

        currentState = State.Infected;
        UpdateColor();
        UpdateAnimatorStage();

        if (points.Count > currentIndex + 1)
            infectedThresholdIndex = Random.Range(currentIndex + 1, points.Count);
        else
            infectedThresholdIndex = -1;
    }

    public void Heal()
    {
        currentState = State.Healthy;
        UpdateColor();
        UpdateAnimatorStage();

        infectionThresholdVisits = Random.Range(5, 11);
        sourceVisitCount = 0;
        infectedThresholdIndex = -1;
    }

    private void UpdateColor()
    {
        if (!sr) return;

        sr.color =
            currentState == State.Healthy ? Color.green :
            currentState == State.Infected ? Color.yellow :
            Color.red;
    }

    private void UpdateAnimatorStage()
    {
        if (animator == null) return;
        animator.SetInteger("Stage", (int)currentState);   // SetInteger для int‑параметра[web:15]
    }

    // ====== ИНИЦИАЛИЗАЦИЯ ======

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
