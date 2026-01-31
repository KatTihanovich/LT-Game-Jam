using UnityEngine;
using UnityEngine.EventSystems;

public class MaskSlotUI : MonoBehaviour, IDropHandler
{
    public string acceptsPieceId;
    public float snapDistance = 80f; // pixels

    private RectTransform slotRect;
    public MaskPuzzleManager manager;

    void Awake()
    {
        slotRect = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        var piece = draggedObj.GetComponent<DraggablePieceUI>();
        if (piece == null || piece.IsLocked) return;

        if (piece.pieceId != acceptsPieceId) return;

        RectTransform pieceRect = draggedObj.GetComponent<RectTransform>();
        float dist = Vector2.Distance(pieceRect.anchoredPosition, slotRect.anchoredPosition);
        if (dist > snapDistance) return;

        piece.SnapAndLockTo(slotRect);
        manager?.CheckComplete();
    }
}
