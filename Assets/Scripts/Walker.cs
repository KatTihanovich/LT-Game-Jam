using UnityEngine;

public class Walker : MonoBehaviour
{
    public float speed = 2f;

    private Vector2 targetPoint;

    [HideInInspector] public float minX;
    [HideInInspector] public float maxX;
    [HideInInspector] public float minY;
    [HideInInspector] public float maxY;

    void Start()
    {
        SetNewTarget();
    }

    void Update()
    {
        MoveToTarget();
    }

    void MoveToTarget()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetPoint) < 0.05f)
        {
            SetNewTarget();
        }
    }

    void SetNewTarget()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        targetPoint = new Vector2(x, y);
    }
}
