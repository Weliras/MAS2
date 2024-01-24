using System.Collections.Concurrent;

namespace cv1;

public class DataCalculations
{
    public static double CalculateAverageNumberOfCommonNeighbors(ConcurrentDictionary<int, int> localNumberOfNeighbors)
    {
        return (double)localNumberOfNeighbors.Values.Sum() / localNumberOfNeighbors.Keys.Count;
    }
    public static int CalculateMaxNumberOfCommonNeighbors(ConcurrentDictionary<int, int> localNumberOfNeighbors)
    {
        return localNumberOfNeighbors.Values.Max();
    }
    public static double CalculateAverageDegree(ConcurrentDictionary<int, int> localDegrees)
    {
        return (double)localDegrees.Values.Sum() / localDegrees.Keys.Count;
    }

    public static int CalculateMaxDegree(ConcurrentDictionary<int, int> localDegrees)
    {
        return localDegrees.Values.Max();
    }

    public static void GenerateClusteringEffect(Graph graph, bool parallel = false)
    {
        if (parallel)
        {
            ConcurrentDictionary<int, double> degreeAvgClusteringCoef = new ConcurrentDictionary<int, double>();
            Parallel.ForEach(graph.LocalDegrees, degree =>
            {
                // Get all clustering coef of 
                List<int> nodesForDegree = graph.LocalDegrees
                    .Where(kv => kv.Value == degree.Value)
                    .Select(kv => kv.Key)
                    .ToList();
                double sum = 0;
                foreach (var node in nodesForDegree)
                {
                    double clusteringCoeffForNode;
                    graph.LocalClusteringCoefficient.TryGetValue(node, out clusteringCoeffForNode);
                    sum += clusteringCoeffForNode;
                }

                degreeAvgClusteringCoef.TryAdd(degree.Value, sum / nodesForDegree.Count);
            });
            
            // Save clustering effect to file and visualise it using python
            using (StreamWriter writer = new StreamWriter("output/clusteringEffect.csv"))
            {
                writer.WriteLine($"Degree;AvgClusteringCoef");
                foreach (var entry in degreeAvgClusteringCoef)
                {
                    writer.WriteLine($"{entry.Key};{entry.Value}");
                }
            }
        }
        else
        {
            ConcurrentDictionary<int, double> degreeAvgClusteringCoef = new ConcurrentDictionary<int, double>();
            foreach (var degree in graph.LocalDegrees)
            {
                // Get all clustering coef of 
                List<int> nodesForDegree = graph.LocalDegrees
                    .Where(kv => kv.Value == degree.Value)
                    .Select(kv => kv.Key)
                    .ToList();
                double sum = 0;
                foreach (var node in nodesForDegree)
                {
                    double clusteringCoeffForNode;
                    graph.LocalClusteringCoefficient.TryGetValue(node, out clusteringCoeffForNode);
                    sum += clusteringCoeffForNode;
                }

                degreeAvgClusteringCoef.TryAdd(degree.Value, sum / nodesForDegree.Count);
            }
            
            // Save clustering effect to file and visualise it using python
            using (StreamWriter writer = new StreamWriter("output/clusteringEffect.csv"))
            {
                writer.WriteLine($"Degree;AvgClusteringCoef");
                foreach (var entry in degreeAvgClusteringCoef)
                {
                    writer.WriteLine($"{entry.Key};{entry.Value}");
                }
            }
        }
        
    }
    public static void GenerateDegreeDistribution(Graph network)
    {
        // Collect degrees
        List<int> degrees = network.LocalDegrees.Values.ToList();

        // Create a histogram of degree counts
        var degreeHistogram = degrees.GroupBy(degree => degree)
                                                      .OrderBy(group => group.Key)
                                                      .Select(group => new { Degree = group.Key, Count = group.Count() })
                                                      .ToList();
        // Save degree, count to file and visualise it using python
        using (StreamWriter writer = new StreamWriter("output/degreeDistribution.csv"))
        {
            writer.WriteLine($"Degree;Count");
            foreach (var entry in degreeHistogram)
            {
                writer.WriteLine($"{entry.Degree};{entry.Count}");
            }
        }
    }

    public static int CalculateLocalDegree(int node, Graph graph)
    {
        return graph.GetNeighbors(node).Count;
    }
    
    public static int CalculateCommonNeighbors(int node, Graph graph)
    {
        int countOfCommonNeighbors = 0;
        
        foreach (int neighbor1 in graph.GetNeighbors(node))
        {
            foreach (int neighbor2 in graph.GetNeighbors(node))
            {
                if (neighbor1 < neighbor2)
                {
                    // Because it is undirected graph
                    var neighbor1Neighbors = graph.GetNeighbors(neighbor1);
                    //var neighbor2Neighbors = graph.GetNeighbors(neighbor2);
                    if (neighbor1Neighbors.Contains(neighbor2))
                    {
                        countOfCommonNeighbors += 1;
                    }
                }
            }
        }

        return countOfCommonNeighbors;
    }
    public static double CalculateLocalClusteringCoefficient(int node, Graph graph)
    {
        List<int> neighbors = graph.GetNeighbors(node);
        int numberOfNeighbors = neighbors.Count;

        if (numberOfNeighbors < 2)
        {
            return 0.0; // Nodes with fewer than 2 neighbors have a clustering coefficient of 0.
        }

        var numberOfConnections = 0;

        // Check for connections between neighbors
        foreach (int neighbor1 in neighbors)
        {
            foreach (int neighbor2 in neighbors)
            {
                if (neighbor1 < neighbor2 && graph.HasEdge(neighbor1, neighbor2))
                {
                    numberOfConnections++;
                }
            }
        }

        return  (2.0 * numberOfConnections) / (numberOfNeighbors * (numberOfNeighbors - 1));
    }

}