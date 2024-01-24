namespace DBLPAsMultilayer;

using System.Collections.Concurrent;

public class GraphInLayer
{
    //  graph
    private ConcurrentDictionary<int, ConcurrentDictionary<int, double>> graph;
    public ConcurrentDictionary<int, List<int>> Simplexes { get; set; }

    // Similarity matrix
    private bool weighted;

    public GraphInLayer()
    {
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        Simplexes = new ConcurrentDictionary<int, List<int>>();
        this.weighted = false;
    }

    public GraphInLayer(bool weighted)
    {
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        Simplexes = new ConcurrentDictionary<int, List<int>>();
        this.weighted = weighted;
    }
    public GraphInLayer(string filePath, bool weighted = false)
    {
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        Simplexes = new ConcurrentDictionary<int, List<int>>();
        this.weighted = weighted;
        
        // Load graph
        Load(filePath);
    }

    public GraphInLayer(GraphInLayer other)
    {
        // Create a new instance
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        this.weighted = other.weighted;
        
        // Copy data from the original object to the new object
        foreach (var outerPair in other.graph)
        {
            var innerDictionary = new ConcurrentDictionary<int, double>();
            foreach (var innerPair in outerPair.Value)
            {
                innerDictionary.TryAdd(innerPair.Key, innerPair.Value);
            }
            graph.TryAdd(outerPair.Key, innerDictionary);
        }
    }
    public GraphInLayer(GraphInLayer other, List<int> selectedNodes)
    {
        // Create a new instance
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        this.weighted = other.weighted;
        
        // Copy data from the original object to the new object
        foreach (var outerPair in other.graph)
        {
            if (!selectedNodes.Contains(outerPair.Key))
                continue;
            var innerDictionary = new ConcurrentDictionary<int, double>();
            foreach (var innerPair in outerPair.Value)
            {
                innerDictionary.TryAdd(innerPair.Key, innerPair.Value);
            }
            graph.TryAdd(outerPair.Key, innerDictionary);
        }
    }
    
    private void Load(string filePath)
    {

        using (StreamReader reader = new StreamReader(filePath))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                // Process the line
                if (line.StartsWith('%'))
                    continue;

                var indices = line.Replace("\n", "").Split();
                int idFrom = int.Parse(indices[0]);
                int idTo = int.Parse(indices[1]);
                
                AddNode(idFrom);
                AddNode(idTo);
                AddUndirectedEdge(idFrom, idTo);
            }
        }
    }
    
    public void Merge(GraphInLayer otherGraphInLayer)
    {
        // Merge nodes and edges
        Parallel.ForEach(otherGraphInLayer.graph, kvp =>
        {
            if (!graph.ContainsKey(kvp.Key))
            {
                // If node from the other graph doesn't exists in current graph, then add him to current graph.
                graph.TryAdd(kvp.Key, new ConcurrentDictionary<int, double>());
            }

            foreach (var edge in kvp.Value)
            {
                // Add weight of the edge from other graph to the first one or update weight (old + new)
                graph[kvp.Key].AddOrUpdate(edge.Key, edge.Value, (_, oldValue) => edge.Value + oldValue);
            }
        });
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
    public void AddNode(int node)
    {
        graph.GetOrAdd(node, _ => new ConcurrentDictionary<int, double>());
    }

    public void RemoveUndirectedEdge(int nodeFrom, int nodeTo)
    {
        graph[nodeFrom].TryRemove(nodeTo, out _);
        graph[nodeTo].TryRemove(nodeFrom, out _);
    }
    public void AddUndirectedEdge(int nodeFrom, int nodeTo, double weight = 1.0)
    {
        // Ensure that the nodes exist in the graph
        //graph.GetOrAdd(nodeFrom, _ => new ConcurrentDictionary<int, int>());
        //graph.GetOrAdd(nodeTo, _ => new ConcurrentDictionary<int, int>());
        
        // Check if the edge already exists. If yes, then add + 1 to its weight.
        if (this.weighted)
        {
            // For weighted
            graph[nodeFrom].AddOrUpdate(nodeTo, weight, (_, oldValue) => oldValue + weight);
            graph[nodeTo].AddOrUpdate(nodeFrom, weight, (_, oldValue) => oldValue + weight);
        }
        else
        {
            // For not weighted
            graph[nodeFrom].GetOrAdd(nodeTo, weight);
            graph[nodeTo].GetOrAdd(nodeFrom, weight);
        }
        
        
    }
    public HashSet<int> GetNeighbors(int node)
    {
        if (graph.TryGetValue(node, out var neighbors))
        {
            return neighbors.Keys.ToHashSet();
        }
        else
        {
            return new HashSet<int>();
        }
    }

    
    public int GetLocalDegree(int node)
    {
        return GetNeighbors(node).Count;
    }
    
}