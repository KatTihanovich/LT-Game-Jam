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
        float minX = bottomLeft.position.x;
        float maxX = topRight.position.x;
        float minY = bottomLeft.position.y;
        float maxY = topRight.position.y;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        Vector2 spawnPos = new Vector2(x, y);

        GameObject obj = Instantiate(
            characterPrefab,
            spawnPos,
            Quaternion.identity
        );

        // Получаем CycleWalker
        CycleWalker walker = obj.GetComponent<CycleWalker>();

        // Передаём границы
        walker.SetBounds(minX, maxX, minY, maxY);
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
