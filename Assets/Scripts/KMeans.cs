using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMeans : MonoBehaviour
{
    // We don't have a dataset so we we generate a random one using these values.
    [SerializeField] private int width = 30;
    [SerializeField] private int depth = 30;
    [SerializeField] private int numberOfPoints;
    [SerializeField] private int numberOfCentroids = 3;

    // GameObject references.
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject centroidPrefab;
    [SerializeField] private Transform pointsHolder;
    [SerializeField] private Transform centroidsHolder;
    [SerializeField] private GameObject doneMessage;

    // For use in task 3.
    [SerializeField] private GameObject[] collectibles;

    // Lists containing points and centroids.
    private List<GameObject> points;
    private List<GameObject> centroids;

    // Each time we generate a new dataset, we will also generate new colours for our clusters that we can work with.
    private List<Color> colours;

    // Keys are centroid gameobjects (clusters), values are lists of gameobjects that represent the points that belong to the cluster.
    private Dictionary<GameObject, List<GameObject>> clusters;

    // This is needed to determine when it's time to stop.
    // If the positions of centroids in the current iteration is the same as the positions from the previous iteration.
    private List<Vector3> previousCentroids;

    private void Start()
    {
        StartKMeansClustering();
    }

    // Method to begin clustering. It is called from Start and from the "Start Clustering" UI button.
    public void StartKMeansClustering()
    {
        // Clear previous clustering data.
        ClearData();

        // Initialisation.
        points = GenerateGameObjects(pointPrefab, numberOfPoints, pointsHolder); // Randomly place points.
        centroids = GenerateGameObjects(centroidPrefab, numberOfCentroids, centroidsHolder); // Randomly place centroids.
        previousCentroids = GetCentroidsList(); // Important for checking for convergence.
        colours = GenerateColors();
        SetColoursToCentroids();

        // Execute the rest of the K-means clustering algorithm.
        Cluster();
    }

    // Iterate the algorithm.
    public void Cluster()
    {
        // Your code here.
    }

    // Destroys all point and centroid GameObjects and disables "Done" message.
    private void ClearData()
    {
        DestroyChildren(pointsHolder);
        DestroyChildren(centroidsHolder);
        doneMessage.SetActive(false);
    }

    // Destroys all the child GameObjects of specified parent GameObject.
    private void DestroyChildren(Transform parent)
    {
        foreach (Transform item in parent)
        {
            Destroy(item.gameObject);
        }
    }

    // Update previous centroids to the positions of the current.
    private void UpdatePreviousCentroids()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            previousCentroids[i] = centroids[i].transform.position;
        }
    }

    // Check if no centroids have changed their position.
    private void CheckForConvergence()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            if (centroids[i].transform.position != previousCentroids[i])
                return;
        }

        doneMessage.SetActive(true);
    }

    // Take the sum of all the positions in the cluster and divide by the number of points (to get the mean average).
    private void CalculateNewCentroids()
    {
        int clusterCounter = 0;
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            Vector3 sumOfPositions = Vector3.zero;

            foreach (GameObject point in cluster.Value)
            {
                sumOfPositions += point.transform.position;
            }

            Vector3 averagePosition = sumOfPositions / cluster.Value.Count;
            centroids[clusterCounter].transform.position = averagePosition;
            clusterCounter++;
        }
    }

    // Set colours to the points from each cluster.
    private void SetColorToClusterPoints()
    {
        int clusterCounter = 0;
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            foreach (GameObject point in cluster.Value)
            {
                point.GetComponent<MeshRenderer>().material.color = colours[clusterCounter];
            }
            clusterCounter++;
        }
    }

    // If there's a cluster with no points, extract the closest point and add it to the empty cluster.
    private void CheckForEmptyClusters()
    {
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            if (cluster.Value.Count == 0)
            {
                GameObject closestPoint = ExtractClosestPointToCluster(cluster.Key.transform.position);
                cluster.Value.Add(closestPoint);
            }
        }
    }

    // Find the closest point (from any cluster) to the centroid of an empty cluster, and add that point to the empty cluster.
    private GameObject ExtractClosestPointToCluster(Vector3 clusterPosition)
    {
        GameObject closestPoint = points[0];
        GameObject clusterThePointBelongsTo = null;
        float minDistance = float.MaxValue;

        // Looping through points is not a good idea because we need to find a cluster that has more than 1 item,
        // that's why we will loop through all the clusters and the points in clusters.
        // We only take the point if the cluster has more than 1 item, otherwise we'd take the item from the cluster that has 1 item,
        // Which means that a cluster would end up with no items and we will have the same problem.
        foreach (KeyValuePair<GameObject, List<GameObject>> cluster in clusters)
        {
            foreach (GameObject point in cluster.Value)
            {
                float distance = Vector3.Distance(point.transform.position, clusterPosition);
                if (distance < minDistance && cluster.Value.Count > 1)
                {
                    closestPoint = point;
                    minDistance = distance;
                    clusterThePointBelongsTo = cluster.Key;
                }
            }
        }

        clusters[clusterThePointBelongsTo].Remove(closestPoint);
        return closestPoint;
    }

    // Construct clusters dictionary.
    private void InitialiseClusters()
    {
        // At this point we will have the centroids already generated
        Dictionary<GameObject, List<GameObject>> result = new Dictionary<GameObject, List<GameObject>>();

        for (int i = 0; i < numberOfCentroids; i++)
        {
            result.Add(centroids[i], new List<GameObject>());
        }

        clusters = result;
    }

    // Add each of the points to the cluster they belong to.
    private void AddPointsToClusters()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 pointPosition = points[i].transform.position;
            float minDistance = float.MaxValue;
            GameObject closestCentroid = centroids[0]; // We can randomly pick any centroid as this will update later.

            for (int j = 0; j < numberOfCentroids; j++)
            {
                float distance = Vector3.Distance(pointPosition, centroids[j].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCentroid = centroids[j];
                }
            }

            clusters[closestCentroid].Add(points[i]);
        }
    }

    // Apply the colours from the list to the points in each centroid.
    private void SetColoursToCentroids()
    {
        for (int i = 0; i < centroids.Count; i++)
        {
            centroids[i].GetComponent<MeshRenderer>().material.color = colours[i];
        }
    }

    // Generates a list of random colours.
    private List<Color> GenerateColors()
    {
        List<Color> result = new List<Color>();

        for (int i = 0; i < numberOfCentroids; i++)
        {
            Color color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            result.Add(color);
        }

        return result;
    }

    // Returns a new list containing all the centroid GameObjects.
    private List<Vector3> GetCentroidsList()
    {
        List<Vector3> result = new List<Vector3>();

        foreach (GameObject item in centroids)
        {
            result.Add(item.transform.position);
        }

        return result;
    }

    // Instantiates and returns a list of GameObjects for the points.
    private List<GameObject> GenerateGameObjects(GameObject prefab, int size, Transform parent)
    {
        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < size; i++)
        {
            float prefabXScale = prefab.transform.localScale.x;
            float positionX = UnityEngine.Random.Range(-width / 2 + prefabXScale, width / 2 - prefabXScale);

            float prefabZScale = prefab.transform.localScale.z;
            float positionZ = UnityEngine.Random.Range(-depth / 2 + prefabZScale, depth / 2 - prefabZScale);

            Vector3 newPosition = new Vector3(positionX, prefab.transform.position.y, positionZ);
            GameObject newGameObject = Instantiate(prefab, newPosition, Quaternion.identity, parent);

            result.Add(newGameObject);
        }

        return result;
    }
}
