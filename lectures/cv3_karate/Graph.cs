namespace cv3_2;

using System.Collections.Concurrent;

public class Graph
{
    public delegate double DelegateSimilarityFunction(int node1, int node2, Graph graph);
    //  graph
    private ConcurrentDictionary<int, ConcurrentDictionary<int, int>> graph;
    // Similarity matrix
    public ConcurrentDictionary<int, ConcurrentDictionary<int, double>> similarityMatrix;
    private bool weighted;
    public ISimilarityFunction SimilarityFunction { get; private set; }

    
    // Method to change the similarityFunctions during runtime
    public void ChangeSimilarityFunctions(ISimilarityFunction newSimilarityFunctions)
    {
        this.SimilarityFunction = newSimilarityFunctions;
    }
    
    public Graph(string filePath, bool weighted = false)
    {
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
        similarityMatrix = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        this.weighted = weighted;
        this.SimilarityFunction = new CommonNeighbors();
        
        // Load graph
        Load(filePath);
    }

    public Graph(Graph other)
    {
        // Create a new instance
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
        similarityMatrix = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        this.weighted = other.weighted;
        this.SimilarityFunction = other.SimilarityFunction;
        
        // Copy data from the original object to the new object
        foreach (var outerPair in other.graph)
        {
            var innerDictionary = new ConcurrentDictionary<int, int>();
            foreach (var innerPair in outerPair.Value)
            {
                innerDictionary.TryAdd(innerPair.Key, innerPair.Value);
            }
            graph.TryAdd(outerPair.Key, innerDictionary);
        }
    }
    public Graph(Graph other, List<int> selectedNodes)
    {
        // Create a new instance
        graph = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
        similarityMatrix = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        this.weighted = other.weighted;
        this.SimilarityFunction = other.SimilarityFunction;
        
        // Copy data from the original object to the new object
        foreach (var outerPair in other.graph)
        {
            if (!selectedNodes.Contains(outerPair.Key))
                continue;
            var innerDictionary = new ConcurrentDictionary<int, int>();
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
    
    public void InitializeSimilarityMatrix()
    {
        // For each node with each node
        foreach (var node1 in Nodes)
        {
            // Add key if doesnt exist
            if (!similarityMatrix.Keys.Contains(node1))
                similarityMatrix.TryAdd(node1, new ConcurrentDictionary<int, double>());
            foreach (var node2 in Nodes)
            {
                if (node2.CompareTo(node1) > 0)
                {
                    // Set a default value
                    similarityMatrix[node1].TryAdd(node2, 0.0);
                    if (!similarityMatrix.Keys.Contains(node2))
                        similarityMatrix.TryAdd(node2, new ConcurrentDictionary<int, double>());
                    similarityMatrix[node2].TryAdd(node1, 0.0);
                }
                // skip if nodes equal
                //if (EqualityComparer<int>.Default.Equals(node1, node2)) continue;
                
            }
            
        }
    }
    public void CalculateSimilarityMatrix()
    {
        // For each node with each node
        foreach (var node1 in similarityMatrix.Keys)
        {
            foreach (var node2 in similarityMatrix[node1].Keys)
            {
                if (node2.CompareTo(node1) > 0)
                {
                    // Calculate value
                    double similarityValue = SimilarityFunction.CalculateSimilarity(node1, node2, this);
                    similarityMatrix[node1][node2] = similarityValue;
                    similarityMatrix[node2][node1] = similarityValue;
                }
            }
            
        }
    }
    public void PredictNewEdges(double threshold)
    {
        foreach (var node1 in similarityMatrix.Keys)
        {
            foreach (var node2 in similarityMatrix[node1].Keys)
            {
                if (node2.CompareTo(node1) > 0 && similarityMatrix[node1][node2] > threshold)
                {
                    AddNode(node1);
                    AddNode(node2);
                    AddUndirectedEdge(node1, node2);
                }
            }
            
        }
    }
    
    public ConcurrentDictionary<string, int> Compare(Graph trueGraph)
    {
        // TP: Exists and predicted
        // FN: Exists and not predicted
        // FP: Not exists and predicted
        // TN: Not exists and not predicted
        ConcurrentDictionary<string, int> confusionMatrix = new ConcurrentDictionary<string, int>();
        confusionMatrix.TryAdd("TP", 0);
        confusionMatrix.TryAdd("FN", 0);
        confusionMatrix.TryAdd("FP", 0);
        confusionMatrix.TryAdd("TN", 0);
        // For each dictionary of nodes in true graph
        Parallel.ForEach(this.Nodes, node1 =>
        {
            foreach (var node2 in this.Nodes)
            {
                if (node2.CompareTo(node1) > 0)
                {
                    // Pred YES, True YES
                    if ( (this.HasEdge(node1, node2) || this.HasEdge(node2, node1)) &&
                         (trueGraph.HasEdge(node1, node2) || trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("TP", 1, (_, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                    // Pred YES, True NO
                    else if ( (this.HasEdge(node1, node2) || this.HasEdge(node2, node1)) &&
                              (!trueGraph.HasEdge(node1, node2) && !trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("FP", 1, (_, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                    // Pred NO, True YES
                    else if ( (!this.HasEdge(node1, node2) && !this.HasEdge(node2, node1)) &&
                              (trueGraph.HasEdge(node1, node2) || trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("FN", 1, (_, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                    // Pred NO, True NO
                    else if ((!this.HasEdge(node1, node2) && !this.HasEdge(node2, node1)) &&
                             (!trueGraph.HasEdge(node1, node2) && !trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("TN", 1, (_, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                }
                
            }
        });
        return confusionMatrix;
    }
    public void Merge(Graph otherGraph)
    {
        // Merge nodes and edges
        Parallel.ForEach(otherGraph.graph, kvp =>
        {
            if (!graph.ContainsKey(kvp.Key))
            {
                // If node from the other graph doesn't exists in current graph, then add him to current graph.
                graph.TryAdd(kvp.Key, new ConcurrentDictionary<int, int>());
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
        graph.GetOrAdd(node, _ => new ConcurrentDictionary<int, int>());
    }

    public void RemoveUndirectedEdge(int nodeFrom, int nodeTo)
    {
        graph[nodeFrom].TryRemove(nodeTo, out _);
        graph[nodeTo].TryRemove(nodeFrom, out _);
    }
    public void AddUndirectedEdge(int nodeFrom, int nodeTo)
    {
        // Ensure that the nodes exist in the graph
        //graph.GetOrAdd(nodeFrom, _ => new ConcurrentDictionary<int, int>());
        //graph.GetOrAdd(nodeTo, _ => new ConcurrentDictionary<int, int>());
        
        // Check if the edge already exists. If yes, then add + 1 to its weight.
        if (this.weighted)
        {
            // For weighted
            graph[nodeFrom].AddOrUpdate(nodeTo, 1, (_, oldValue) => oldValue + 1);
            graph[nodeTo].AddOrUpdate(nodeFrom, 1, (_, oldValue) => oldValue + 1);
        }
        else
        {
            // For not weighted
            graph[nodeFrom].GetOrAdd(nodeTo, 1);
            graph[nodeTo].GetOrAdd(nodeFrom, 1);
        }
        
        
    }
    public ConcurrentDictionary<int, int> GetNeighbors(int node)
    {
        if (graph.TryGetValue(node, out var neighbors))
        {
            return neighbors;
        }
        else
        {
            return new ConcurrentDictionary<int, int>();
        }
    }


    public void RemovePartOfEdges(double part = 0.2)
    {
        int numberOfEdgesToRemove = (int)(EdgeCount * 0.2);
        Console.WriteLine($"Removing randomly {numberOfEdgesToRemove} edges.");

        Random rng = new Random();

        int i = 0;
        while (i < numberOfEdgesToRemove)
        {
            // randomly select node
            int nodeFrom = Nodes[rng.Next(0, NodesCount)];
            var neigbors = GetNeighbors(nodeFrom);
            if (neigbors.Count <= 0)
                continue;

            int nodeTo = neigbors.Keys.ToList()[rng.Next(0, neigbors.Count)];
            // Remove edge
            RemoveUndirectedEdge(nodeFrom, nodeTo);
            i++;
        }

            
        
    }
    public int GetLocalDegree(int node)
    {
        return GetNeighbors(node).Keys.Count;
    }
    public Graph PredictLinks(double threshold)
    {
        // make copy of graph
        var predictedGraph = new Graph(this);
        // Create similarity matrix - DoK
        predictedGraph.InitializeSimilarityMatrix();
        predictedGraph.CalculateSimilarityMatrix();
        predictedGraph.PredictNewEdges(threshold);
        

        return predictedGraph;
    }

    
}