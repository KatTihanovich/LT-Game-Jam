using UnityEngine;

public class Source : MonoBehaviour
{
    public static Source Instance;

    [Header("HP")]
    public float maxHP = 175f;
    public float currentHP = 175f;

    [Header("Вклад состояний за одно посещение")]
    public float healthyDelta  = +1.75f;
    public float infectedDelta = 0f;
    public float sickDelta     = -1.75f;

    [Header("Лимиты состояний")]
    public int maxInfected = 6;
    public int maxSick     = 6;

    private float sessionTime = 0f;

    private int healthyCount;
    private int infectedCount;
    private int sickCount;

    // === АНИМАЦИЯ ИСТОЧНИКА ===
    private Animator animator;
    private readonly int stageHash = Animator.StringToHash("Stages"); // int‑параметр в Animator [web:130][web:136]

    private void Awake()
    {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        UpdateAnimationStage();
        PrintHP();
    }

    private void Update()
    {
        sessionTime += Time.deltaTime;
    }

    public void OnWalkerVisitedSource(CycleWalker walker)
    {
        if (walker == null) return;

        RecountStates();

        float delta = 0f;

        switch (walker.currentState)
        {
            case CycleWalker.State.Healthy:
                delta = healthyDelta;
                break;
            case CycleWalker.State.Infected:
                delta = infectedDelta;
                break;
            case CycleWalker.State.Sick:
                delta = sickDelta;
                break;
        }

        currentHP = Mathf.Clamp(currentHP + delta, 0f, maxHP);

        // обновляем анимационный Stage каждый раз, когда меняется HP
        UpdateAnimationStage();

        Debug.Log(
            $"[SOURCE] t={sessionTime:F1}s | {walker.name} ({walker.currentState}) " +
            $"ΔHP={delta:+0.00;-0.00;0.00}, HP={currentHP:0.00}/{maxHP}"
        );
    }

    private void UpdateAnimationStage()
    {
        if (animator == null) return;

        // переводим HP в 0–1
        float hp01 = maxHP > 0 ? currentHP / maxHP : 0f;

        int stage = 0;

        // твои пороги:
        // 0 HP        -> 0
        // 25%  и выше -> 1
        // 50%  и выше -> 2
        // 75%  и выше -> 3
        // 100% и выше -> 4
        if (hp01 >= 1.0f)
            stage = 4;
        else if (hp01 >= 0.75f)
            stage = 3;
        else if (hp01 >= 0.50f)
            stage = 2;
        else if (hp01 >= 0.25f)
            stage = 1;
        else
            stage = 0;

        Debug.Log($"[SOURCE] Updating animation stage to {stage} (HP={currentHP:0.00}/{maxHP})");
        animator.SetInteger(stageHash, stage); // дёргаем анимацию по int‑параметру [web:130][web:136]
    }

    private void RecountStates()
    {
        healthyCount  = 0;
        infectedCount = 0;
        sickCount     = 0;

        var walkers = FindObjectsOfType<CycleWalker>();
        foreach (var w in walkers)
        {
            if (w == null) continue;

            switch (w.currentState)
            {
                case CycleWalker.State.Healthy:
                    healthyCount++;
                    break;
                case CycleWalker.State.Infected:
                    infectedCount++;
                    break;
                case CycleWalker.State.Sick:
                    sickCount++;
                    break;
            }
        }
    }

    [ContextMenu("Print Stats")]
    public void PrintStats()
    {
        RecountStates();

        float infectionFrequencyPercentPerSec = sessionTime * 0.06f;

        Debug.Log(
            $"[SOURCE STATS] t={sessionTime:F1}s | " +
            $"Healthy={healthyCount}, Infected={infectedCount}/{maxInfected}, Sick={sickCount}/{maxSick} | " +
            $"HP={currentHP:0.00}/{maxHP} | " +
            $"freq≈{infectionFrequencyPercentPerSec:0.00}%/сек (по формуле)"
        );
    }

    private void PrintHP()
    {
        Debug.Log($"[SOURCE] HP: {currentHP:0.00}/{maxHP}");
    }
}
