using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggablePieceUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string pieceId;

    private RectTransform rect;
    private Canvas rootCanvas;
    private RectTransform rootCanvasRect;
    private CanvasGroup canvasGroup;

    private Vector2 startAnchoredPos;
    private Transform startParent;

    private Camera uiCamera;

    public bool IsLocked { get; private set; }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
        rootCanvasRect = rootCanvas.GetComponent<RectTransform>();

        uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsLocked) return;

        startAnchoredPos = rect.anchoredPosition;
        startParent = transform.parent;

        // Move to top so it renders above other UI while dragging
        transform.SetParent(rootCanvas.transform, true);

        // Let raycasts go "through" so slots receive OnDrop
        canvasGroup.blocksRaycasts = false;

        // Immediately place correctly under pointer
        SetAnchoredPositionFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsLocked) return;
        SetAnchoredPositionFromPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsLocked) return;

        canvasGroup.blocksRaycasts = true;

        // If no slot locked us, return back
        if (!IsLocked)
        {
            transform.SetParent(startParent, true);
            rect.anchoredPosition = startAnchoredPos;
        }
    }

    private void SetAnchoredPositionFromPointer(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvasRect,
                eventData.position,
                uiCamera,
                out localPoint))
        {
            rect.anchoredPosition = localPoint;
        }
    }

    public void SnapAndLockTo(RectTransform slotTransform)
    {
        IsLocked = true;

        transform.SetParent(slotTransform, false);

        rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;

        // Match slot size exactly
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotTransform.rect.width);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotTransform.rect.height);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

}
