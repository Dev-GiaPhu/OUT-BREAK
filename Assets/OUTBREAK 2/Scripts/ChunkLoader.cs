using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChunkEntry
{
    public GameObject prefab;
    [Range(1, 100)]
    public int weight = 10;
}

public class ChunkLoader : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Start Chunk Pool (Safe)")]
    public List<GameObject> startChunkPrefabs;

    [Header("Enemy Chunk Pool (Weighted)")]
    public List<ChunkEntry> chunkEntries;

    [Header("Chunk Settings")]
    public int chunkSize = 20;
    public int loadRadius = 1;

    [Header("Safe Radius")]
    public int safeRadius = 0;

    private Dictionary<Vector2Int, GameObject> loadedChunks =
        new Dictionary<Vector2Int, GameObject>();

    // ================= START =================

    void Start()
    {
        SpawnStartChunk();
    }

    void Update()
    {
        if (player == null) return;

        Vector2Int playerChunk = GetChunkCoord(player.position);
        LoadAround(playerChunk);
        Debug.Log("Player chunk: " + GetChunkCoord(player.position));

    }

    // ================= LOAD =================

    void LoadAround(Vector2Int center)
    {
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int coord = center + new Vector2Int(x, y);

                if (!loadedChunks.ContainsKey(coord))
                {
                    SpawnChunk(coord);
                }
            }
        }
    }

    // ================= SPAWN =================

    void SpawnStartChunk()
    {
        Vector2Int startCoord = Vector2Int.zero;

        if (loadedChunks.ContainsKey(startCoord)) return;
        if (startChunkPrefabs.Count == 0) return;

        GameObject prefab =
            startChunkPrefabs[Random.Range(0, startChunkPrefabs.Count)];

        GameObject chunk = Instantiate(
            prefab,
            new Vector3( 0, 0, 0),
            Quaternion.identity
        );

        chunk.transform.SetParent(transform);
        loadedChunks.Add(startCoord, chunk);

        Debug.Log($"Spawn START chunk: {prefab.name}");
    }

    void SpawnChunk(Vector2Int coord)
    {
        if (coord == Vector2Int.zero) return;

        Vector3 worldPos = new Vector3(
            (coord.x) * chunkSize,
            (coord.y) * chunkSize,
            0
        );

        // 🟢 SAFE ZONE → dùng start chunk pool
        if (Mathf.Abs(coord.x) <= safeRadius &&
            Mathf.Abs(coord.y) <= safeRadius)
        {
            if (startChunkPrefabs.Count == 0) return;

            GameObject prefab =
                startChunkPrefabs[Random.Range(0, startChunkPrefabs.Count)];

            GameObject safeChunk = Instantiate(
                prefab,
                worldPos,
                Quaternion.identity
            );

            safeChunk.transform.SetParent(transform);
            loadedChunks.Add(coord, safeChunk);
            return;
        }

        // 🔴 ENEMY ZONE → weighted random
        ChunkEntry entry = GetRandomChunk();
        if (entry == null) return;

        GameObject chunk = Instantiate(
            entry.prefab,
            worldPos,
            Quaternion.identity
        );

        chunk.transform.SetParent(transform);
        loadedChunks.Add(coord, chunk);
    }

    // ================= RANDOM =================

    ChunkEntry GetRandomChunk()
    {
        int totalWeight = 0;
        foreach (var c in chunkEntries)
            totalWeight += c.weight;

        int rand = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var c in chunkEntries)
        {
            current += c.weight;
            if (rand < current)
                return c;
        }

        return null;
    }

    // ================= UTILS =================

    Vector2Int GetChunkCoord(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x + chunkSize * 0.5f) / chunkSize);
        int y = Mathf.FloorToInt((worldPos.y + chunkSize * 0.5f) / chunkSize);
        return new Vector2Int(x, y);
    }

    // ================= GIZMOS =================

    void OnDrawGizmos()
    {
        if (player == null) return;

        Vector2Int center = GetChunkCoord(player.position);

        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int coord = center + new Vector2Int(x, y);

                Vector3 pos = new Vector3(
                    coord.x * chunkSize,
                    coord.y * chunkSize,
                    0
                );

                Gizmos.color =
                    (Mathf.Abs(coord.x) <= safeRadius &&
                     Mathf.Abs(coord.y) <= safeRadius)
                    ? Color.green
                    : Color.red;

                Gizmos.DrawWireCube(
                    pos,
                    new Vector3(chunkSize, chunkSize, 0.1f)
                );
            }
        }
    }
}
