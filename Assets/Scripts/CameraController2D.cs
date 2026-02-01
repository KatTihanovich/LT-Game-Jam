using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [Header("Drag pan (ПКМ)")]
    public float dragSpeed = 1f;

    [Header("Follow dragged walker")]
    public float followLerp = 10f;
    private CycleWalker followTarget;

    private Camera cam;
    private bool isRightDragging = false;
    private Vector3 lastMouseWorldPos;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleFollowDraggedWalker();
    }
    
    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -cam.transform.position.z; 
        return cam.ScreenToWorldPoint(mousePos);
    }


    void HandleFollowDraggedWalker()
    {
        if (followTarget == null)
            return;

        if (!followTarget.IsDragged)
        {
            followTarget = null;
            return;
        }

        Vector3 targetPos = followTarget.transform.position;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followLerp * Time.deltaTime
        );
    }

    public void SetFollowTarget(CycleWalker walker)
    {
        followTarget = walker;
    }
}
