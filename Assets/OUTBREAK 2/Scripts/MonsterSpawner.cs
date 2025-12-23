using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MonsterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MonsterData
    {
        public GameObject prefab;
        [Range(0f, 100f)] 
        [Tooltip("Tỉ lệ % xuất hiện trên mỗi ô (0-100)")]
        public float chancePercent = 10f; 
        
        [Min(0)] public int maxCount = 0;
    }

    // List để quản lý việc dọn dẹp vì không dùng Parent
    [SerializeField, HideInInspector]
    private List<GameObject> spawnedMonsters = new List<GameObject>();

    [Header("Monster Pool")]
    [Tooltip("Thứ tự từ trên xuống dưới là ưu tiên xét trước nếu nhiều quái cùng trúng tỉ lệ.")]
    public List<MonsterData> monsters = new List<MonsterData>();

    [Header("Grid (Local Space)")]
    public Vector2 areaSize = new Vector2(10, 10);
    public float cellSize = 1f;

    [Header("Random Settings")]
    public bool RandomSeed = true;
    public int seed = 0;
    public bool randomRotation = false;

    [ContextMenu("Generate Monsters")]
    public void Generate()
    {
        if (monsters.Count == 0) return;

        Clear();

        if (RandomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        Random.InitState(seed);

        Dictionary<MonsterData, int> spawnedCount = new Dictionary<MonsterData, int>();

        int xCount = Mathf.FloorToInt(areaSize.x / cellSize);
        int yCount = Mathf.FloorToInt(areaSize.y / cellSize);
        List<Vector2Int> allPoints = new List<Vector2Int>();

        for (int x = 0; x < xCount; x++)
            for (int y = 0; y < yCount; y++)
                allPoints.Add(new Vector2Int(x, y));

        for (int i = 0; i < allPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, allPoints.Count);
            Vector2Int temp = allPoints[i];
            allPoints[i] = allPoints[randomIndex];
            allPoints[randomIndex] = temp;
        }

        foreach (Vector2Int point in allPoints)
        {
            foreach (var monster in monsters)
            {
                if (monster.prefab == null) continue;

                spawnedCount.TryGetValue(monster, out int count);
                if (monster.maxCount > 0 && count >= monster.maxCount) continue;

                float roll = Random.Range(0f, 100f);

                if (roll <= monster.chancePercent)
                {
                    // Giữ nguyên công thức tính của bạn
                    Vector3 relativePos = new Vector3(
                        point.x * cellSize + (cellSize / 2f),
                        point.y * cellSize,
                        0
                    );

                    Vector3 worldPos = transform.TransformPoint(relativePos);
                    SpawnMonster(monster, worldPos);

                    if (!spawnedCount.ContainsKey(monster)) spawnedCount[monster] = 0;
                    spawnedCount[monster]++;
                    
                    break; 
                }
            }
        }
    }

    void SpawnMonster(MonsterData monster, Vector3 worldPos)
    {
        GameObject obj = null;
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            obj = (GameObject)PrefabUtility.InstantiatePrefab(monster.prefab);
            Undo.RegisterCreatedObjectUndo(obj, "Spawn Monster");
        } else {
            obj = Instantiate(monster.prefab);
        }
#else
        obj = Instantiate(monster.prefab);
#endif
        obj.transform.position = worldPos;
        if (randomRotation) obj.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        
        spawnedMonsters.Add(obj);
    }

    public void Clear()
    {
        for (int i = spawnedMonsters.Count - 1; i >= 0; i--) {
            if (spawnedMonsters[i] != null) {
#if UNITY_EDITOR
                if (!Application.isPlaying) Undo.DestroyObjectImmediate(spawnedMonsters[i]);
                else Destroy(spawnedMonsters[i]);
#else
                Destroy(spawnedMonsters[i]);
#endif
            }
        }
        spawnedMonsters.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Vector3 size = new Vector3(areaSize.x, areaSize.y, 0);
        Gizmos.DrawWireCube(size / 2f, size);
        
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        for (float x = 0; x <= areaSize.x; x += cellSize)
            Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, areaSize.y, 0));
        for (float y = 0; y <= areaSize.y; y += cellSize)
            Gizmos.DrawLine(new Vector3(0, y, 0), new Vector3(areaSize.x, y, 0));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonsterSpawner))]
public class MonsterSpawnerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true);

        do {
            if (prop.name == "seed") {
                SerializedProperty randomSeed = serializedObject.FindProperty("RandomSeed");
                if (!randomSeed.boolValue) EditorGUILayout.PropertyField(prop);
            } else {
                EditorGUILayout.PropertyField(prop, true);
            }
        } while (prop.NextVisible(false));
        
        EditorGUILayout.Space();
        MonsterSpawner spawner = (MonsterSpawner)target;
        if (GUILayout.Button("Spawn Monsters", GUILayout.Height(30))) spawner.Generate();
        if (GUILayout.Button("Clear Monsters")) spawner.Clear();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif