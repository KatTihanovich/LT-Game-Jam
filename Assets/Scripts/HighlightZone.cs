using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;   // важно для SkeletonAnimation [web:170][web:177]

[RequireComponent(typeof(BoxCollider2D))]
public class HighlightZoneSpine : MonoBehaviour
{
    public int highlightOrder = 30;

    private Dictionary<Renderer, int> originalOrders = new();

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ищем SkeletonAnimation у персонажа
        var skeleton = other.GetComponent<SkeletonAnimation>();
        if (skeleton == null) return;

        Renderer rend = skeleton.GetComponent<Renderer>();
        if (rend == null) return;

        if (originalOrders.ContainsKey(rend)) return;

        originalOrders[rend] = rend.sortingOrder;
        rend.sortingOrder = highlightOrder;   // меняем Order in Layer [web:170][web:177]
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var skeleton = other.GetComponent<SkeletonAnimation>();
        if (skeleton == null) return;

        Renderer rend = skeleton.GetComponent<Renderer>();
        if (rend == null) return;

        if (originalOrders.TryGetValue(rend, out int original))
        {
            rend.sortingOrder = original;
            originalOrders.Remove(rend);
        }
    }
}
