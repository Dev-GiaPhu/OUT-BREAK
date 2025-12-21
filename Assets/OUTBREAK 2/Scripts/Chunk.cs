using UnityEngine;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    [Header("Check Points")]
    public Transform AxisTop;
    public Transform AxisBottom;
    public Transform AxisLeft;
    public Transform AxisRight;

    [Header("Chunk Settings")]
    public List<GameObject> chunkPrefabs;
    public Vector2 checkBoxSize = new Vector2(1.8f, 1.8f);
    public float checkOffset = 1.9f;

    private Transform mapParent;

    void Awake()
    {
        mapParent = GameObject.FindWithTag("Map").transform;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger hit: " + collision.name);

        if (collision.CompareTag("Player"))
        {
            LoadChunksAround();
        }
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("Trigger hit: " + collision.name);

        if (!collision.CompareTag("Player")) return;

        LoadChunksAround();
        Debug.Log("Triger Stay");
    }

    void LoadChunksAround()
    {
        TrySpawn(AxisTop.position, Vector2.up);
        TrySpawn(AxisBottom.position, Vector2.down);
        TrySpawn(AxisLeft.position, Vector2.left);
        TrySpawn(AxisRight.position, Vector2.right);
    }

    void TrySpawn(Vector2 basePos, Vector2 dir)
    {
        Vector2 checkPos = basePos + dir * checkOffset;

        if (IsChunkLoaded(checkPos)) return;

        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Count)];
        GameObject newChunk = Instantiate(prefab, basePos, Quaternion.identity);
        newChunk.transform.SetParent(mapParent);
    }

    bool IsChunkLoaded(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapBox(
            pos,
            checkBoxSize,
            0f,
            LayerMask.GetMask("Chunk")
        );
        Debug.Log("Checking at " + pos);
        return hit != null;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (AxisTop == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(AxisTop.position + Vector3.up * checkOffset, checkBoxSize);
        Gizmos.DrawWireCube(AxisBottom.position + Vector3.down * checkOffset, checkBoxSize);
        Gizmos.DrawWireCube(AxisLeft.position + Vector3.left * checkOffset, checkBoxSize);
        Gizmos.DrawWireCube(AxisRight.position + Vector3.right * checkOffset, checkBoxSize);
    }
#endif
}
