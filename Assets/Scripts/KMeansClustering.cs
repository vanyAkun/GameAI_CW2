using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMeansClustering : MonoBehaviour
{
    // This is your data set that you'll need to input manually.
    private Vector2[] dataSet = new Vector2[]
   {
    new Vector2(1.0f, 1.0f),
    new Vector2(1.0f, 6.0f),
    new Vector2(2.0f, 1.0f),
    new Vector2(3.0f, 9.0f),
    new Vector2(3.0f, 10.0f),
    new Vector2(4.0f, 6.0f),
    new Vector2(5.0f, 6.0f),
    new Vector2(7.0f, 2.0f),
    new Vector2(8.0f, 1.0f),
    new Vector2(8.0f, 9.0f),
    new Vector2(9.0f, 1.0f),
    new Vector2(9.0f, 9.0f),
    new Vector2(9.0f, 10.0f),
    new Vector2(10.0f, 3.0f),
    new Vector2(10.0f, 5.0f)
   };

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject centroidPrefab;
    [SerializeField] private int numberOfClusters = 3;
    [SerializeField] private bool useManhattanDistance = false;

    private List<Vector2> centroids = new List<Vector2>();
    private int currentIteration = 0;
    private Dictionary<int, List<Vector2>> clusters;
    [SerializeField] private Color[] clusterColors;

    private void Start()
    {
        if (clusterColors.Length < numberOfClusters)
        {
            Debug.LogError("Not enough colors provided for the number of clusters!");
            return;
        }

        InitializeCentroids();
        StartCoroutine(PerformKMeansClustering());
    }

    private void InitializeCentroids()
    {
        // Simple initialization of centroids to the first 'numberOfClusters' points
        for (int i = 0; i < numberOfClusters; i++)
        {
            centroids.Add(dataSet[i]);
        }
    }

    private IEnumerator PerformKMeansClustering()
    {
        while (currentIteration < 3)
        {
            clusters = AssignPointsToClusters();
            UpdateCentroids();

            // Visualization in Unity
            VisualizeClusters();
            VisualizeCentroids();

            currentIteration++;
            yield return new WaitForSeconds(1f); // Delay to visualize the steps
        }
    }

    private Dictionary<int, List<Vector2>> AssignPointsToClusters()
    {
        var newClusters = new Dictionary<int, List<Vector2>>();

        // Initialize clusters
        for (int i = 0; i < numberOfClusters; i++)
        {
            newClusters[i] = new List<Vector2>();
        }

        // Assign each point to the nearest centroid
        foreach (var point in dataSet)
        {
            float minDistance = float.MaxValue;
            int closestCentroidIndex = 0;

            for (int i = 0; i < centroids.Count; i++)
            {
                float distance = useManhattanDistance
                    ? ManhattanDistance(point, centroids[i])
                    : EuclideanDistance(point, centroids[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroidIndex = i;
                }
            }

            newClusters[closestCentroidIndex].Add(point);
        }

        return newClusters;
    }

    private void UpdateCentroids()
    {
        for (int i = 0; i < numberOfClusters; i++)
        {
            Vector2 newCentroid = new Vector2();
            foreach (var point in clusters[i])
            {
                newCentroid += point;
            }
            centroids[i] = newCentroid / clusters[i].Count;
        }
    }

    private void VisualizeClusters()
    {
        // Clear previous visualizations
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Visualize clusters with points
        int clusterIndex = 0;
        foreach (var cluster in clusters)
        {
            foreach (var point in cluster.Value)
            {
                GameObject enemy = Instantiate(enemyPrefab, new Vector3(point.x, 0, point.y), Quaternion.identity, transform);
                var renderer = enemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = clusterColors[clusterIndex]; // Apply color based on cluster index
                }
            }
            clusterIndex++;
        }
    }

    


    private void VisualizeCentroids()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            GameObject centroidGO = Instantiate(centroidPrefab, new Vector3(centroids[i].x, 0, centroids[i].y), Quaternion.identity, transform);
            var renderer = centroidGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = clusterColors[i]; // Apply the same color as the cluster
            }
        }
    }


    private float EuclideanDistance(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    private float ManhattanDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
