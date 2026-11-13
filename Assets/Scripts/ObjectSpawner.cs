using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectSpawner : MonoBehaviour
{
    public enum ObjectType { Enemy, Fruit }

    public Tilemap tilemap;
    public GameObject[] objectPrefabs;
    [Range(0f, 1f)] public float bananaProbability = 0.5f;
    [Range(0f, 1f)] public float enemyProbability = 0.1f;
    public int maxObjects = 10;
    public float fruitLifeTime = 10f;
    public float spawnInterval = 0.5f;

    [Header("Spawn placement")]
    [SerializeField] private float fruitHalfHeight = 0.25f;
    [SerializeField] private float enemyHalfHeight = 0.48f;
    [SerializeField] private float surfacePadding = 0.03f;
    [SerializeField] private float playerClearance = 1.5f;
    [SerializeField] private float objectClearance = 1.25f;
    [SerializeField] private string noSpawnObjectName = "NotSpawn";

    private readonly List<Vector3> validSpawnPositions = new List<Vector3>();
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<Collider2D> noSpawnColliders = new List<Collider2D>();
    private bool isSpawning;

    private void Start()
    {
        GameController.OnReset += LevelChange;
        Tilemap activeGround = FindActiveGround();
        if (activeGround != null)
            tilemap = activeGround;

        GatherValidPositions();
        StartSpawnLoopIfNeeded();
    }

    private void Update()
    {
        if (tilemap == null || !tilemap.gameObject.activeInHierarchy)
        {
            Tilemap activeGround = FindActiveGround();
            if (activeGround != null && activeGround != tilemap)
            {
                tilemap = activeGround;
                LevelChange();
            }
        }

        if (tilemap == null || !tilemap.gameObject.activeInHierarchy)
            return;

        StartSpawnLoopIfNeeded();
    }

    private Tilemap FindActiveGround()
    {
        return FindObjectsByType<Tilemap>(FindObjectsSortMode.None)
            .FirstOrDefault(map => map.gameObject.name == "Ground" && map.gameObject.activeInHierarchy);
    }

    private void StartSpawnLoopIfNeeded()
    {
        if (!isSpawning && tilemap != null && tilemap.gameObject.activeInHierarchy &&
            ActiveObjectCount() < maxObjects)
        {
            StartCoroutine(SpawnObjectIfNeeded());
        }
    }

    private void LevelChange()
    {
        StopAllCoroutines();
        isSpawning = false;
        DestroyAllSpawnedObjects();

        Tilemap activeGround = FindActiveGround();
        if (activeGround != null)
            tilemap = activeGround;

        GatherValidPositions();
        StartSpawnLoopIfNeeded();
    }

    private int ActiveObjectCount()
    {
        spawnedObjects.RemoveAll(item => item == null);
        return spawnedObjects.Count;
    }

    private IEnumerator SpawnObjectIfNeeded()
    {
        isSpawning = true;
        while (tilemap != null && tilemap.gameObject.activeInHierarchy &&
               ActiveObjectCount() < maxObjects && validSpawnPositions.Count > 0)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;
    }

    private ObjectType RandomObjectType()
    {
        return Random.value <= enemyProbability ? ObjectType.Enemy : ObjectType.Fruit;
    }

    private void SpawnObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length <= (int)ObjectType.Fruit)
        {
            Debug.LogError("ObjectSpawner needs an Enemy prefab at index 0 and a Fruit prefab at index 1.", this);
            return;
        }

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        ObjectType objectType = RandomObjectType();
        GameObject prefab = objectPrefabs[(int)objectType];
        if (prefab == null)
        {
            Debug.LogError($"ObjectSpawner prefab at index {(int)objectType} is not assigned.", this);
            return;
        }

        List<int> candidateIndices = Enumerable.Range(0, validSpawnPositions.Count)
            .OrderBy(_ => Random.value).ToList();

        foreach (int candidateIndex in candidateIndices)
        {
            Vector3 surfacePosition = validSpawnPositions[candidateIndex];
            float objectHalfHeight = objectType == ObjectType.Enemy ? enemyHalfHeight : fruitHalfHeight;
            float objectHalfWidth = objectType == ObjectType.Enemy ? 0.4f : 0.25f;
            Vector3 spawnPosition = surfacePosition + Vector3.up * (objectHalfHeight + surfacePadding);
            if (player != null && Vector2.Distance(player.position, spawnPosition) < playerClearance)
                continue;
            if (IsInsideNoSpawnArea(spawnPosition, objectHalfWidth, objectHalfHeight))
                continue;
            if (PositionHasObject(spawnPosition))
                continue;

            validSpawnPositions.RemoveAt(candidateIndex);
            GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            spawnedObjects.Add(spawnedObject);

            if (objectType == ObjectType.Fruit)
                StartCoroutine(DestroyObjectAfterTime(spawnedObject, surfacePosition, fruitLifeTime));

            return;
        }
    }

    private IEnumerator DestroyObjectAfterTime(GameObject spawnedObject, Vector3 originalPosition, float time)
    {
        yield return new WaitForSeconds(time);
        if (spawnedObject != null)
        {
            spawnedObjects.Remove(spawnedObject);
            Destroy(spawnedObject);
        }

        if (!validSpawnPositions.Any(position => Vector3.Distance(position, originalPosition) < 0.1f))
            validSpawnPositions.Add(originalPosition);
    }

    private void DestroyAllSpawnedObjects()
    {
        foreach (GameObject spawnedObject in spawnedObjects)
        {
            if (spawnedObject != null)
                Destroy(spawnedObject);
        }
        spawnedObjects.Clear();
    }

    private bool PositionHasObject(Vector3 position)
    {
        return spawnedObjects.Any(spawnedObject => spawnedObject != null &&
            Vector2.Distance(spawnedObject.transform.position, position) < objectClearance);
    }

    public void GatherValidPositions()
    {
        validSpawnPositions.Clear();
        noSpawnColliders.Clear();
        if (tilemap == null)
        {
            Debug.LogWarning("ObjectSpawner has no active Ground Tilemap.", this);
            return;
        }

        RefreshNoSpawnColliders();

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    Vector3Int cell = new Vector3Int(x, y, z);
                    if (!tilemap.HasTile(cell) || tilemap.HasTile(cell + Vector3Int.up))
                        continue;

                    Vector3 cellBottom = tilemap.CellToWorld(cell);
                    Vector3 surfacePosition = cellBottom + new Vector3(0.5f, 1f, 0f);
                    if (!IsInsideNoSpawnArea(surfacePosition, 0f, 0f))
                        validSpawnPositions.Add(surfacePosition);
                }
            }
        }
    }

    private void RefreshNoSpawnColliders()
    {
        Transform levelRoot = FindContainingLevelRoot();

        foreach (Transform child in levelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name != noSpawnObjectName || !child.gameObject.activeInHierarchy)
                continue;

            foreach (Collider2D areaCollider in child.GetComponentsInChildren<Collider2D>())
            {
                if (areaCollider.enabled && !noSpawnColliders.Contains(areaCollider))
                    noSpawnColliders.Add(areaCollider);
            }
        }
    }

    private Transform FindContainingLevelRoot()
    {
        GameController gameController = FindFirstObjectByType<GameController>();
        if (gameController != null && gameController.levels != null)
        {
            foreach (GameObject level in gameController.levels)
            {
                if (level != null && (tilemap.transform == level.transform ||
                                      tilemap.transform.IsChildOf(level.transform)))
                {
                    return level.transform;
                }
            }
        }

        for (Transform current = tilemap.transform; current != null; current = current.parent)
        {
            if (current.name.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase))
                return current;
        }

        return tilemap.transform.root;
    }

    private bool IsInsideNoSpawnArea(Vector3 position, float halfWidth, float halfHeight)
    {
        noSpawnColliders.RemoveAll(areaCollider => areaCollider == null || !areaCollider.enabled ||
                                                   !areaCollider.gameObject.activeInHierarchy);

        foreach (Collider2D areaCollider in noSpawnColliders)
        {
            Bounds bounds = areaCollider.bounds;
            bool overlapsHorizontalRange = position.x + halfWidth >= bounds.min.x &&
                                           position.x - halfWidth <= bounds.max.x;
            bool isAtOrAboveCollider = position.y + halfHeight >= bounds.min.y;
            if (overlapsHorizontalRange && isAtOrAboveCollider)
                return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        GameController.OnReset -= LevelChange;
    }
}
