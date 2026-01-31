using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class QuarantineZone : MonoBehaviour
{
    public int maxCapacity = 2;

    public Color zoneColor = new Color(1f, 0f, 0f, 0.25f);
    public Color borderColor = Color.red;

    public Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
    public float healCooldown = 3f;

    private List<CycleWalker> inside = new List<CycleWalker>();
    private BoxCollider2D box;
    private bool canHeal = true;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    // ====== НОВОЕ ======

    public bool ContainsPoint(Vector2 point)
    {
        return box.OverlapPoint(point);
    }

    public bool IntersectsLine(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = Physics2D.Linecast(from, to);
        return hit.collider == box;
    }

    // ====== ТРИГГЕРЫ ======

    void OnTriggerEnter2D(Collider2D other)
    {
        CycleWalker walker = other.GetComponent<CycleWalker>();
        if (walker == null) return;

        if (walker.IsDragged){
        if (walker.currentState == CycleWalker.State.Healthy)
            return;

        if (inside.Count < maxCapacity)
            PutInQuarantine(walker);
        }    
    }


    void OnTriggerExit2D(Collider2D other)
    {
        CycleWalker walker = other.GetComponent<CycleWalker>();
        if (walker == null) return;

        if (inside.Contains(walker))
        {
            inside.Remove(walker);
            walker.ExitQuarantine();
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
            w.ExitQuarantine();

        inside.Clear();
    }

    public void HealWalker(CycleWalker walker)
    {
        if (!canHeal) return;
        if (walker == null) return;
        if (!inside.Contains(walker)) return;

        inside.Remove(walker);
        walker.Heal();
        walker.ExitQuarantine();

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
