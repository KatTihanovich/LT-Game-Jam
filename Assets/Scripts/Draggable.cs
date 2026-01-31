using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    private CycleWalker walker;

    // Границы движения (как в SetBounds)
    private float minX, maxX, minY, maxY;

    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    void Start()
    {
        cam = Camera.main;
        walker = GetComponent<CycleWalker>();
    }

    void OnMouseDown()
    {
        isDragging = true;

        if (walker != null)
        {
            walker.enabled = false;
            walker.SetDragged(true);
        }

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        offset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 targetPos = mousePos + offset;

        // Ограничиваем внутри зоны
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        transform.position = targetPos;
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (walker != null)
        {
            walker.enabled = true;
            walker.SetDragged(false);
        }
    }
}
