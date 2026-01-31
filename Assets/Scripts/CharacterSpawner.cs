using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject characterPrefab;
    public int count = 10;

    // Углы зоны
    public Transform bottomLeft;
    public Transform topRight;

    private void Start()
    {
        SpawnAll();
    }

    private void SpawnAll()
    {
        List<CycleWalker> allWalkers = new List<CycleWalker>();

        for (int i = 0; i < count; i++)
        {
            CycleWalker w = SpawnOne();
            if (w != null)
                allWalkers.Add(w);
        }

        // 2–4 заражённых (но не больше общего количества)
        int infectedCount = Random.Range(5, 8); 
        infectedCount = Mathf.Clamp(infectedCount, 0, allWalkers.Count);

        // Перемешиваем список, чтобы выбрать случайных
        for (int i = 0; i < allWalkers.Count; i++)
        {
            int j = Random.Range(i, allWalkers.Count);
            var tmp = allWalkers[i];
            allWalkers[i] = allWalkers[j];
            allWalkers[j] = tmp;
        }

        for (int i = 0; i < infectedCount; i++)
        {
            allWalkers[i].Infect();
        }
    }

    private CycleWalker SpawnOne()
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

    Draggable drag = obj.GetComponent<Draggable>();
        if (drag != null)
        {
            drag.SetBounds(minX, maxX, minY, maxY);
        }

        // Получаем CycleWalker
        CycleWalker walker = obj.GetComponent<CycleWalker>();
        if (walker != null)
        {
            // Передаём границы
            walker.SetBounds(minX, maxX, minY, maxY);
        }
        else
        {
            Debug.LogError("[CharacterSpawner] На префабе нет CycleWalker!");
        }

        return walker;
    }

    // Визуализация зоны
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
