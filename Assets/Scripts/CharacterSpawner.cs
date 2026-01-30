using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject characterPrefab;
    public int count = 10;

    // Углы зоны
    public Transform bottomLeft;
    public Transform topRight;

    void Start()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        float x = Random.Range(
            bottomLeft.position.x,
            topRight.position.x
        );

        float y = Random.Range(
            bottomLeft.position.y,
            topRight.position.y
        );

        Vector2 spawnPos = new Vector2(x, y);

        GameObject obj = Instantiate(
            characterPrefab,
            spawnPos,
            Quaternion.identity
        );

        Walker walker = obj.GetComponent<Walker>();

        walker.minX = bottomLeft.position.x;
        walker.maxX = topRight.position.x;
        walker.minY = bottomLeft.position.y;
        walker.maxY = topRight.position.y;
    }

    // Визуализация зоны
    void OnDrawGizmos()
    {
        if (bottomLeft && topRight)
        {
            Gizmos.color = Color.green;

            Vector3 center = (bottomLeft.position + topRight.position) / 2;
            Vector3 size = topRight.position - bottomLeft.position;

            Gizmos.DrawWireCube(center, size);
        }
    }
}
