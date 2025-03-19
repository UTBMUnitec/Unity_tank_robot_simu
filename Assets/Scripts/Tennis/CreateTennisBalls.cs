using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTennisBall : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject tennisBallPrefab;
    public int numberOfBalls = 10;
    
    [Header("Ball Reference")]
    public List<Transform> tennisBallsList = new List<Transform>();
    
    private Bounds spawnBounds;

    private void Awake()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            spawnBounds = collider.bounds;
        }
    }
    
    private void Start()
    {
        if (tennisBallsList.Count == 0)
        {
            SpawnTennisBalls();
        }
    }
    
    private void SpawnTennisBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                Random.Range(spawnBounds.min.z, spawnBounds.max.z)
            );
            
            GameObject newBall = Instantiate(tennisBallPrefab, randomPosition, Quaternion.identity);
            newBall.name = "TennisBall_" + i;
            
            tennisBallsList.Add(newBall.transform);
        }
    }
}
