using System.Collections.Concurrent;

namespace cv1;

public class Graph
{
    private ConcurrentDictionary<int, ConcurrentDictionary<int, bool>> graph;
    public ConcurrentDictionary<int, int> LocalDegrees { get; }
    public ConcurrentDictionary<int, double> LocalClusteringCoefficient { get; }
    public ConcurrentDictionary<int, int> LocalCommonNeighbors { get; }
    public Graph()
    {
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, bool>>();
        LocalDegrees = new ConcurrentDictionary<int, int>();
        LocalClusteringCoefficient = new ConcurrentDictionary<int, double>();
        LocalCommonNeighbors = new ConcurrentDictionary<int, int>();
    }
    
    public int NodesCount
    {
        get
        {
            return graph.Count;
        }
    }

    public int EdgeCount
    {
        get
        {
            // Divide by 2 for undirected graph
            return graph.Values.Sum(edges => edges.Count) / 2;
        }
    }

    public List<int> Nodes
    {
        get
        {
            return graph.Keys.ToList();
        }
    }
    
    public bool HasNode(int node)
    {
        return graph.ContainsKey(node);
    }
    
    public bool HasEdge(int fromNode, int toNode)
    {
        return HasNode(fromNode) && graph[fromNode].ContainsKey(toNode);
    }
    public void AddEdge(int nodeFrom, int nodeTo)
    {
        // Ensure that the nodes exist in the graph
        graph.GetOrAdd(nodeFrom, _ => new ConcurrentDictionary<int, bool>());
        graph.GetOrAdd(nodeTo, _ => new ConcurrentDictionary<int, bool>());

        // Add the edge by setting both directions to true (assuming an undirected graph)
        graph[nodeFrom].TryAdd(nodeTo, true);
        graph[nodeTo].TryAdd(nodeFrom, true);
    }

    public List<int> GetNeighbors(int node)
    {
        if (graph.TryGetValue(node, out var neighbors))
        {
            return neighbors.Select(item => item.Key).ToList();
        }
        else
        {
            return new List<int>();
        }
    }

    public void CalculateLocalDegrees(bool parallel = false)
    {
        if (parallel)
        {
            Parallel.ForEach(Nodes, node =>
            {
                LocalDegrees.TryAdd(node, DataCalculations.CalculateLocalDegree(node, this));
            });
        }
        else
        {
            foreach (var node in Nodes)
            {
                LocalDegrees.TryAdd(node, DataCalculations.CalculateLocalDegree(node, this));
            }
        }
    }

    public void CalculateLocalCommonNeighbors(bool parallel = false)
    {
        if (parallel)
        {
            Parallel.ForEach(Nodes, node =>
            {
                LocalCommonNeighbors.TryAdd(node, DataCalculations.CalculateCommonNeighbors(node, this));
            });
        }
        else
        {
            foreach (var node in Nodes)
            {
                LocalCommonNeighbors.TryAdd(node, DataCalculations.CalculateCommonNeighbors(node, this));
            }
        }
    }
    public void CalculateLocalClusteringCoefficients(bool parallel = false)
    {
        if (parallel)
        {
            Parallel.ForEach(Nodes, node =>
            {
                LocalClusteringCoefficient.TryAdd(node, DataCalculations.CalculateLocalClusteringCoefficient(node, this));
            });
        }
        else
        {
            foreach (var node in Nodes)
            {
                LocalClusteringCoefficient.TryAdd(node, DataCalculations.CalculateLocalClusteringCoefficient(node, this));
            }
        }
        
    }
}