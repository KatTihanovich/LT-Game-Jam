using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject[] characterPrefabs;   // 5 префабов
    public int perPrefabCount = 4;          // по 4 каждого

    public Transform bottomLeft;
    public Transform topRight;

    private void Start()
    {
        SpawnAll();
    }

    private void SpawnAll()
    {
        List<CycleWalker> allWalkers = new List<CycleWalker>();

        // перебираем каждый префаб
        foreach (var prefab in characterPrefabs)
        {
            for (int i = 0; i < perPrefabCount; i++)
            {
                CycleWalker w = SpawnOne(prefab);
                if (w != null)
                    allWalkers.Add(w);
            }
        }

        // считаем общее количество
        int totalCount = allWalkers.Count;

        // например, 5–7 заражённых, но не больше общего количества
//        int infectedCount = Random.Range(5, 8);
//        infectedCount = Mathf.Clamp(infectedCount, 0, totalCount);
        int infectedCount = totalCount;
        // перемешиваем список (Фишер–Йетс)
        for (int i = 0; i < totalCount; i++)
        {
            int j = Random.Range(i, totalCount);
            var tmp = allWalkers[i];
            allWalkers[i] = allWalkers[j];
            allWalkers[j] = tmp;
        }

        // заражаем первых infectedCount
        for (int i = 0; i < infectedCount; i++)
        {
            allWalkers[i].Infect();
            allWalkers[i].Sick();
        }
    }

    private CycleWalker SpawnOne(GameObject prefab)
    {
        float minX = bottomLeft.position.x;
        float maxX = topRight.position.x;
        float minY = bottomLeft.position.y;
        float maxY = topRight.position.y;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        Vector2 spawnPos = new Vector2(x, y);

        GameObject obj = Instantiate(
            prefab,
            spawnPos,
            Quaternion.identity
        );

        Draggable drag = obj.GetComponent<Draggable>();
        if (drag != null)
        {
            drag.SetBounds(minX, maxX, minY, maxY);
        }

        CycleWalker walker = obj.GetComponent<CycleWalker>();
        if (walker != null)
        {
            walker.SetBounds(minX, maxX, minY, maxY);
        }
        else
        {
            Debug.LogError("[CharacterSpawner] На префабе нет CycleWalker!");
        }

        return walker;
    }

    private void OnDrawGizmos()
    {
        if (bottomLeft && topRight)
        {
            Gizmos.color = Color.green;
            Vector3 center = (bottomLeft.position + topRight.position) / 2f;
            Vector3 size = topRight.position - bottomLeft.position;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
