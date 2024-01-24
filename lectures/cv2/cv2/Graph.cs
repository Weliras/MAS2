namespace cv2;

using System.Collections.Concurrent;

public class Graph<T> where T : struct, IComparable<T>
{   
    // Weighted graph
    private ConcurrentDictionary<T, ConcurrentDictionary<T, int>> graph;
    // Simplexes
    public ConcurrentDictionary<int, List<T>> Simplexes { get; set; }
    public ConcurrentDictionary<T, int> LocalDegrees { get; }
    public ConcurrentDictionary<T, int> LocalWeightedDegrees { get; }
    public ConcurrentDictionary<T, double> LocalClusteringCoefficient { get; }
    public ConcurrentDictionary<T, int> LocalCommonNeighbors { get; }
    public Graph()
    {
        graph = new ConcurrentDictionary<T, ConcurrentDictionary<T, int>>();
        Simplexes = new ConcurrentDictionary<int, List<T>>();
        LocalDegrees = new ConcurrentDictionary<T, int>();
        LocalWeightedDegrees = new ConcurrentDictionary<T, int>();
        LocalClusteringCoefficient = new ConcurrentDictionary<T, double>();
        LocalCommonNeighbors = new ConcurrentDictionary<T, int>();
    }
    
    public void Merge(Graph<T> otherGraph)
    {
        foreach (var node in otherGraph.graph)
        {
            if (!graph.ContainsKey(node.Key))
            {
                // If node from the other graph doesn't exists in current graph, then add him to current graph.
                graph.TryAdd(node.Key, new ConcurrentDictionary<T, int>());
            }

            foreach (var edge in node.Value)
            {
                // Add weight of the edge from other graph to the first one or update weight (old + new)
                graph[node.Key].AddOrUpdate(edge.Key, edge.Value, (key, oldValue) => edge.Value + oldValue);
            }
        }
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

    public List<T> Nodes
    {
        get
        {
            return graph.Keys.ToList();
        }
    }
    
    public bool HasNode(T node)
    {
        return graph.ContainsKey(node);
    }
    
    public bool HasEdge(T fromNode, T toNode)
    {
        return HasNode(fromNode) && graph[fromNode].ContainsKey(toNode);
    }

    public void AddNode(T node)
    {
        graph.GetOrAdd(node, _ => new ConcurrentDictionary<T, int>());
    }
    public void AddUndirectedEdge(T nodeFrom, T nodeTo)
    {
        // Ensure that the nodes exist in the graph
        //graph.GetOrAdd(nodeFrom, _ => new ConcurrentDictionary<T, int>());
        //graph.GetOrAdd(nodeTo, _ => new ConcurrentDictionary<T, int>());
        
        // Check if the edge already exists. If yes, then add + 1 to its weight.
        graph[nodeFrom].AddOrUpdate(nodeTo, 1, (_, oldValue) => oldValue + 1);
        graph[nodeTo].AddOrUpdate(nodeFrom, 1, (_, oldValue) => oldValue + 1);
    }
    public ConcurrentDictionary<T, int> GetNeighbors(T node)
    {
        if (graph.TryGetValue(node, out var neighbors))
        {
            return neighbors;
        }
        else
        {
            return new ConcurrentDictionary<T, int>();
        }
    }

    public (int, double) CalculateSimplexWithMaxAvgWeight(bool parallel = false)
    {
        ConcurrentDictionary<int, double> result = new ConcurrentDictionary<int, double>();
        if (parallel)
        {
            Parallel.ForEach(Simplexes, simplex =>
            {
                double _sum = 0.0;
                double _count = 0.0;
                foreach (var node in simplex.Value)
                {
                    int wDegree;
                    if (LocalWeightedDegrees.TryGetValue(node, out wDegree))
                    {
                        _sum += wDegree;
                        _count++;
                    }
                    
                }

                result.TryAdd(simplex.Key, _sum / _count);
            });
        
            return (result.FirstOrDefault(x => x.Value == result.Values.Max()).Key, result.Values.Max());
        }
        else
        {
            return (result.FirstOrDefault(x => x.Value == result.Values.Max()).Key, result.Values.Max());
        }
    }
    public void CalculateLocalWeightedDegrees(bool parallel = false)
    {
        if (parallel)
        {
            Parallel.ForEach(Nodes, node =>
            {
                LocalWeightedDegrees.TryAdd(node, DataCalculations.CalculateLocalWeightedDegree(node, this));
            });
        }
        else
        {
            foreach (var node in Nodes)
            {
                LocalWeightedDegrees.TryAdd(node, DataCalculations.CalculateLocalWeightedDegree(node, this));
            }
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