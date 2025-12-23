using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ChunkItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ItemData
    {
        public GameObject prefab;
        [Range(0f, 100f)] 
        [Tooltip("Tỉ lệ % xuất hiện trên mỗi ô (0-100)")]
        public float chancePercent = 10f; 
        
        [Min(0)] public int maxCount = 0;
    }

    [Header("Item Parent")]
    public Transform itemParent;

    [Header("Item Pool")]
    [Tooltip("Thứ tự từ trên xuống dưới là ưu tiên xét trước nếu nhiều item cùng trúng tỉ lệ.")]
    public List<ItemData> items = new List<ItemData>();

    [Header("Grid (Local Space)")]
    public Vector2 areaSize = new Vector2(10, 10);
    public float cellSize = 1f;

    [Header("Random Settings")]
    public bool RandomSeed = true;
    public int seed = 0;
    public bool randomRotation = false;

    [ContextMenu("Generate Items")]
    public void Generate()
    {
        if (itemParent == null || items.Count == 0) return;

        Clear();

        if (RandomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        Random.InitState(seed);

        Dictionary<ItemData, int> spawnedCount = new Dictionary<ItemData, int>();

        // 1. Tạo danh sách tất cả các ô có thể có
        int xCount = Mathf.FloorToInt(areaSize.x / cellSize);
        int yCount = Mathf.FloorToInt(areaSize.y / cellSize);
        List<Vector2Int> allPoints = new List<Vector2Int>();

        for (int x = 0; x < xCount; x++)
            for (int y = 0; y < yCount; y++)
                allPoints.Add(new Vector2Int(x, y));

        // 2. TRỘN NGẪU NHIÊN DANH SÁCH Ô (Shuffle)
        // Điều này đảm bảo dù 100% trúng thì vị trí vẫn là ngẫu nhiên khắp map
        for (int i = 0; i < allPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, allPoints.Count);
            Vector2Int temp = allPoints[i];
            allPoints[i] = allPoints[randomIndex];
            allPoints[randomIndex] = temp;
        }

        // 3. Xét từng ô đã được trộn để spawn
        foreach (Vector2Int point in allPoints)
        {
            foreach (var item in items)
            {
                if (item.prefab == null) continue;

                // Kiểm tra giới hạn số lượng
                spawnedCount.TryGetValue(item, out int count);
                if (item.maxCount > 0 && count >= item.maxCount) continue;

                // Tung xúc xắc độc lập cho từng item theo %
                float roll = Random.Range(0f, 1000f);

                if (roll <= item.chancePercent)
                {
                    // Tính vị trí chuẩn khớp Gizmo
                    Vector3 relativePos = new Vector3(
                        point.x * cellSize + (cellSize / 2f),
                        point.y * cellSize,
                        0
                    );

                    Vector3 worldPos = transform.TransformPoint(relativePos);
                    SpawnObject(item, worldPos);

                    if (!spawnedCount.ContainsKey(item)) spawnedCount[item] = 0;
                    spawnedCount[item]++;
                    
                    // Khi một ô đã có đồ thì không xét item tiếp theo cho ô đó nữa
                    break; 
                }
            }
        }
    }

    void SpawnObject(ItemData item, Vector3 worldPos)
    {
        GameObject obj = null;
#if UNITY_EDITOR
        if (!Application.isPlaying) obj = (GameObject)PrefabUtility.InstantiatePrefab(item.prefab, itemParent);
        else obj = Instantiate(item.prefab, itemParent);
#else
        obj = Instantiate(item.prefab, itemParent);
#endif
        obj.transform.position = worldPos;
        if (randomRotation) obj.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
    }

    public void Clear()
    {
        if (itemParent == null) return;
        for (int i = itemParent.childCount - 1; i >= 0; i--) {
            GameObject child = itemParent.GetChild(i).gameObject;
#if UNITY_EDITOR
            if (!Application.isPlaying) Undo.DestroyObjectImmediate(child);
            else Destroy(child);
#else
            Destroy(child);
#endif
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(areaSize.x, areaSize.y, 0);
        Gizmos.DrawWireCube(size / 2f, size);
        
        // Vẽ lưới mờ
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        for (float x = 0; x <= areaSize.x; x += cellSize)
            Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, areaSize.y, 0));
        for (float y = 0; y <= areaSize.y; y += cellSize)
            Gizmos.DrawLine(new Vector3(0, y, 0), new Vector3(areaSize.x, y, 0));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChunkItemSpawner))]
public class ChunkItemSpawnerEditor : Editor {
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true);

        do
        {
            if (prop.name == "seed")
            {
                SerializedProperty randomSeed =
                    serializedObject.FindProperty("RandomSeed");

                if (!randomSeed.boolValue)
                    EditorGUILayout.PropertyField(prop);
            }
            else
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }
        while (prop.NextVisible(false));
        
        EditorGUILayout.Space();
        ChunkItemSpawner spawner = (ChunkItemSpawner)target;
        if (GUILayout.Button("Generate Items", GUILayout.Height(30))) spawner.Generate();
        if (GUILayout.Button("Clear Items")) spawner.Clear();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif