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
    public float bananaProbability = 0.5f; // Probability of spawning a banana (0 to 1)
    public float enemyProbability = 0.1f; // Probability of spawning an enemy (0 to 1)
    public int maxObjects = 10; // Maximum number of objects to spawn
    public float fruitLifeTime = 10f; // Time in seconds before a fruit disappears
    public float spawnInterval = 0.5f; // Time in seconds between spawns

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isSpawning = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GatherValidPositions();
        StartCoroutine(SpawnObjectIfNeed());
        GameController.OnReset += LevelChange;
    }

    // Update is called once per frame
    void Update()
    {
        if(!tilemap.gameObject.activeInHierarchy)
        {
            // level change
            LevelChange();
        }
        if (!isSpawning && ActiveObjectCount() < maxObjects)
        {
            StartCoroutine(SpawnObjectIfNeed());
        }
    }

    private void LevelChange()
    {
        tilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        GatherValidPositions();
        DestroyAllSpawnedObjects();
    }

    private int ActiveObjectCount()
    {
        // Remove any null references from the list
        spawnedObjects.RemoveAll(item => item == null);
        return spawnedObjects.Count;
    }

    private IEnumerator SpawnObjectIfNeed()
    {
        isSpawning = true;
        while (ActiveObjectCount() < maxObjects)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;
    }

    private ObjectType RandomObjectType()
    {
        float randomValue = Random.value;
        if (randomValue <= enemyProbability)
        {
            return ObjectType.Enemy;
        }
        else if (randomValue <= ( enemyProbability + bananaProbability))
        {
            return ObjectType.Fruit;
        }
        else
        {
            return ObjectType.Fruit; // Default to fruit if no other type is selected
        }
    }

    private void SpawnObject()
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning("No valid spawn positions available.");
            return;
        }
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while(!validPositionFound && validSpawnPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 potentialPosition = validSpawnPositions[randomIndex];
            Vector3 leftPosition = potentialPosition + Vector3.left;
            Vector3 rightPosition = potentialPosition + Vector3.right;

            if (!PositionHasObject(leftPosition) && !PositionHasObject(rightPosition))
            {
                spawnPosition = potentialPosition;
                validPositionFound = true;
            }
            else
            {
                validSpawnPositions.RemoveAt(randomIndex);
            }
        }

        if (validPositionFound)
        {
            ObjectType objectType = RandomObjectType();
            GameObject gameObject = Instantiate(objectPrefabs[(int)objectType], spawnPosition, Quaternion.identity);
            spawnedObjects.Add(gameObject);

            // Destroy fruit after time
            if(objectType != ObjectType.Enemy)
            {
                StartCoroutine(DestroyObjectAfterTime(gameObject, fruitLifeTime));
            }
        }
    }

    private IEnumerator DestroyObjectAfterTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);
        if (gameObject)
        {
            spawnedObjects.Remove(gameObject);
            validSpawnPositions.Add(gameObject.transform.position);
            Destroy(gameObject);
        }
    }

    private void DestroyAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }
    private bool PositionHasObject(Vector3 positionToCheck)
    {
        return spawnedObjects.Any(checkObj => checkObj && Vector3.Distance(checkObj.transform.position, positionToCheck) < 1.0f);
    }
    public void GatherValidPositions()
    {
        validSpawnPositions.Clear();
        BoundsInt boundsInt = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(boundsInt);
        Vector3 start = tilemap.CellToWorld(new Vector3Int(boundsInt.xMin, boundsInt.yMin, 0));

        for(int x = 0; x < boundsInt.size.x; x++)
        {
            for(int y = 0; y < boundsInt.size.y; y++)
            {
                TileBase tile = allTiles[x + y * boundsInt.size.x];
                if(tile != null)
                {
                    Vector3 place = start + new Vector3(x + 0.5f, y + 2f, 0);
                    validSpawnPositions.Add(place);
                }
            }
        }
    }
}
