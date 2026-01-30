using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    private Walker walker;

    void Start()
    {
        cam = Camera.main;
        walker = GetComponent<Walker>();
    }

    void OnMouseDown()
    {
        isDragging = true;

        // Выключаем авто-движение
        if (walker != null)
            walker.enabled = false;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        offset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        transform.position = mousePos + offset;
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Включаем движение обратно
        if (walker != null)
            walker.enabled = true;
    }
}
