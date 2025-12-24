using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ChunkGrassSpawner : MonoBehaviour
{
    [Header("Grass Parent")]
    public Transform plantParent;

    [Header("Grass Prefabs")]
    public List<GameObject> grassPrefabs;

    [Header("Spawn Area (Local Space)")]
    public Vector2 areaSize = new Vector2(20, 20);

    [Header("Density")]
    [Range(0f, 1f)]
    public float density = 0.3f;

    [Header("Grid Settings")]
    public float cellSize = 1f;

    [Header("Random Seed")]
    public bool RandomSeed = true;
    public int seed = 0;

    public bool randomRotation = true;
    

    // ================= CORE =================
    [ContextMenu("Generate Grass")]
    public void Generate()
    {
        if (plantParent == null) return;
        if (grassPrefabs.Count == 0) return;

        Clear();

        if (RandomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        Random.InitState(seed);

        int xCount = Mathf.FloorToInt(areaSize.x / cellSize);
        int yCount = Mathf.FloorToInt(areaSize.y / cellSize);

        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                if (Random.value > density) continue;

                Vector3 localPos = new Vector3(
                    -areaSize.x / 2 + x * cellSize + cellSize / 2,
                    -areaSize.y / 2 + y * cellSize + cellSize / 2,
                    0
                );

                GameObject prefab =
                    grassPrefabs[Random.Range(0, grassPrefabs.Count)];

                GameObject grass = Instantiate(prefab, plantParent);
                grass.transform.localPosition = localPos;

                if (randomRotation)
                {
                    grass.transform.localRotation =
                        Quaternion.Euler(0, 0, Random.Range(0, 360));
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RegisterCreatedObjectUndo(grass, "Spawn Grass");
#endif
            }
        }
    }

    [ContextMenu("Clear Grass")]
    public void Clear()
    {
        if (plantParent == null) return;

        for (int i = plantParent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(plantParent.GetChild(i).gameObject);
            else
#endif
                DestroyImmediate(plantParent.GetChild(i).gameObject);
        }
    }

    void Start()
    {
        Generate();
    }

    // ================= GIZMOS =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(-transform.position, areaSize);
    }
}

#if UNITY_EDITOR
// ================= CUSTOM INSPECTOR =================
[CustomEditor(typeof(ChunkGrassSpawner))]
public class ChunkGrassSpawnerEditor : Editor
{
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
        ChunkGrassSpawner spawner = (ChunkGrassSpawner)target;
        if (GUILayout.Button("Generate Items", GUILayout.Height(30))) spawner.Generate();
        if (GUILayout.Button("Clear Items")) spawner.Clear();
        serializedObject.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
