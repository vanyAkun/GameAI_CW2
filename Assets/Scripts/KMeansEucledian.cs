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
    

    private List<Vector2> centroids = new List<Vector2>();
    private int currentIteration = 0;
    private Dictionary<int, List<Vector2>> clusters;
    [SerializeField] private Color[] clusterColors;
    [SerializeField] private float scaleFactor = 3.0f;

    private void Start()
    {
        if (clusterColors.Length < numberOfClusters)
        {
            Debug.LogError("Not enough colours");
            return;
        }

        InitializeCentroids();
        VisualizeCentroids();
    }
    public void Iteration()
    {
        if (currentIteration < 3)
        {
            clusters = AssignPointsToClusters();
            UpdateCentroids();

            
            VisualizeClusters();
            VisualizeCentroids();

            currentIteration++;
        }
        else
        {
            Debug.Log("Max iterations reached");
        }
    }
    private void InitializeCentroids()
    {
        for (int i = 0; i < numberOfClusters; i++)
        {
            centroids.Add(dataSet[i]);
        }
    }

    private Dictionary<int, List<Vector2>> AssignPointsToClusters()
    {
        var newClusters = new Dictionary<int, List<Vector2>>();

        // Initialize clusters with empty lists
        for (int i = 0; i < numberOfClusters; i++)
        {
            newClusters.Add(i, new List<Vector2>());
        }

        // Assign each point to the nearest centroid using Euclidean distance
        foreach (var point in dataSet)
        {
            float minDistance = float.MaxValue;
            int closestCentroidIndex = 0;

            for (int i = 0; i < centroids.Count; i++)
            {
                float distance = EuclideanDistance(point, centroids[i]);

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
                Vector3 scaledPoint = new Vector3(point.x * scaleFactor, 0, point.y * scaleFactor);
                GameObject enemy = Instantiate(enemyPrefab, scaledPoint, Quaternion.identity, transform);
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
            Vector3 scaledCentroid = new Vector3(centroids[i].x * scaleFactor, 0, centroids[i].y * scaleFactor);
            GameObject centroidGO = Instantiate(centroidPrefab, scaledCentroid, Quaternion.identity, transform);
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


}
