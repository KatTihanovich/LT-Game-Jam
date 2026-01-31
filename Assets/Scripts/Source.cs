using UnityEngine;

public class Source : MonoBehaviour
{
    public static Source Instance;

    [Header("HP")]
    public float maxHP = 175f;
    public float currentHP = 175f;

    [Header("Вклад состояний за одно посещение")]
    public float healthyDelta  = +1.75f;  // здоровый
    public float infectedDelta = 0f;  // инфицированный
    public float sickDelta     = -1.75f;   // больной

    [Header("Лимиты состояний")]
    public int maxInfected = 6;  // максимум инфицированных
    public int maxSick     = 6;  // максимум заражённых/больных

    // Время с начала сессии (сек)
    private float sessionTime = 0f;

    // Статистика по состояниям
    private int healthyCount;
    private int infectedCount;
    private int sickCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentHP = currentHP;
        PrintHP();
    }

    private void Update()
    {
        sessionTime += Time.deltaTime;

    }

    public void OnWalkerVisitedSource(CycleWalker walker)
    {
        if (walker == null) return;

        // Обновляем счётчики состояний по всей сцене
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

        Debug.Log(
            $"[SOURCE] t={sessionTime:F1}s | {walker.name} ({walker.currentState}) " +
            $"ΔHP={delta:+0.00;-0.00;0.00}, HP={currentHP:0.00}/{maxHP}"
        );
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

    /// <summary>
    /// Вручную вывести расширенную статистику (можно вызывать из Debug кнопки).
    /// </summary>
    [ContextMenu("Print Stats")]
    public void PrintStats()
    {
        RecountStates();

        float infectionFrequencyPercentPerSec = sessionTime * 0.06f; // как у тебя в формуле

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
