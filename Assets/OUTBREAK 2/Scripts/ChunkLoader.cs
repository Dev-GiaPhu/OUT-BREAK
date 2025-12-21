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

    [Header("Start Chunk (Safe Area)")]
    public GameObject startChunkPrefab;

    [Header("Chunk Pool (Weighted)")]
    public List<ChunkEntry> chunkEntries;

    [Header("Chunk Settings")]
    public int chunkSize = 20;
    public int loadRadius = 1;

    private Dictionary<Vector2Int, GameObject> loadedChunks =
        new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        SpawnStartChunk();
    }

    void Update()
    {
        if (player == null) return;

        Vector2Int playerChunk = GetChunkCoord(player.position);
        LoadAround(playerChunk);
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

    void SpawnStartChunk()
    {
        Vector2Int startCoord = Vector2Int.zero;

        if (loadedChunks.ContainsKey(startCoord)) return;

        GameObject chunk = Instantiate(
            startChunkPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        chunk.transform.SetParent(transform);
        loadedChunks.Add(startCoord, chunk);

        Debug.Log("Spawn Start Chunk");
    }

    void SpawnChunk(Vector2Int coord)
    {
        // Không random đè lên chunk khởi đầu
        if (coord == Vector2Int.zero) return;

        Vector3 worldPos = new Vector3(
            coord.x * chunkSize,
            coord.y * chunkSize,
            0
        );

        ChunkEntry entry = GetRandomChunk();
        if (entry == null) return;

        GameObject chunk = Instantiate(
            entry.prefab,
            worldPos,
            Quaternion.identity
        );

        chunk.transform.SetParent(transform);
        loadedChunks.Add(coord, chunk);

        Debug.Log($"Spawn Chunk {entry.prefab.name} at {coord}");
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
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int y = Mathf.FloorToInt(worldPos.y / chunkSize);
        return new Vector2Int(x, y);
    }
}
