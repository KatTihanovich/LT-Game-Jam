using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class QuarantineZone : MonoBehaviour
{
    public int maxCapacity = 2;

    public Color zoneColor = new Color(1f, 0f, 0f, 0.25f); // обычный цвет
    public Color borderColor = Color.red;

    public Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.25f); // цвет во время КД
    public float healCooldown = 3f;

    private List<CycleWalker> inside = new List<CycleWalker>();
    private BoxCollider2D box;
    private bool canHeal = true;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CycleWalker walker = other.GetComponent<CycleWalker>();
        if (walker == null) return;

        if (inside.Count < maxCapacity)
        {
            PutInQuarantine(walker);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        CycleWalker walker = other.GetComponent<CycleWalker>();
        if (walker == null) return;

        if (inside.Contains(walker))
        {
            ReleaseOne();
            inside.Remove(walker);
        }
    }

    void PutInQuarantine(CycleWalker walker)
    {
        if (inside.Contains(walker)) return;

        inside.Add(walker);
        walker.EnterQuarantine();
    }

    public void ReleaseOne()
    {
        if (inside.Count == 0) return;

        CycleWalker w = inside[0];
        inside.RemoveAt(0);
        w.ExitQuarantine();
    }

    public void ReleaseAll()
    {
        foreach (var w in inside)
        {
            w.ExitQuarantine();
        }

        inside.Clear();
    }

    // Лечение конкретного персонажа в карантине (с учётом КД)
    public void HealWalker(CycleWalker walker)
    {
        if (!canHeal) return;
        if (walker == null) return;
        if (!inside.Contains(walker)) return;

        inside.Remove(walker);
        walker.Heal();
        walker.ExitQuarantine();

        Debug.Log($"[Quarantine] {walker.name} вылечен и выпущен из карантина");

        StartCoroutine(HealCooldownRoutine());
    }

    private IEnumerator HealCooldownRoutine()
    {
        canHeal = false;
        yield return new WaitForSeconds(healCooldown);
        canHeal = true;
    }

    void OnMouseDown()
    {
        ReleaseOne();
    }

    // Визуализация зоны + цвет КД
    void OnDrawGizmos()
    {
        if (!box) box = GetComponent<BoxCollider2D>();

        Gizmos.color = canHeal ? zoneColor : cooldownColor;

        Vector3 center = transform.position + (Vector3)box.offset;
        Vector3 size = box.size;
        Gizmos.DrawCube(center, size);
        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(center, size);
    }
}
