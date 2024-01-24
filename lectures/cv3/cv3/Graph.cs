namespace cv2;

using System.Collections.Concurrent;

public class Graph<T> where T : struct, IComparable<T>
{
    public delegate double DelegateSimilarityFunction(T node1, T node2, Graph<T> graph);

    public ISimilarityFunction<T> SimilarityFunction { get; private set; }

    // Method to change the similarityFunctions during runtime
    public void ChangeSimilarityFunctions(ISimilarityFunction<T> newSimilarityFunctions)
    {
        this.SimilarityFunction = newSimilarityFunctions;
    }
    
    // Weighted graph
    private ConcurrentDictionary<T, ConcurrentDictionary<T, int>> graph;
    // Similarity matrix
    public ConcurrentDictionary<T, ConcurrentDictionary<T, double>> similarityMatrix;
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
        similarityMatrix = new ConcurrentDictionary<T, ConcurrentDictionary<T, double>>();
        this.SimilarityFunction = new CommonNeighbors<T>();

    }

    public void InitializeSimilarityMatrix()
    {
        // For each node with each node
        foreach (var node1 in Nodes)
        {
            // Add key if doesnt exist
            if (!similarityMatrix.Keys.Contains(node1))
                similarityMatrix.TryAdd(node1, new ConcurrentDictionary<T, double>());
            foreach (var node2 in Nodes)
            {
                if (node2.CompareTo(node1) > 0)
                {
                    // Set a default value
                    similarityMatrix[node1].TryAdd(node2, 0.0);
                    if (!similarityMatrix.Keys.Contains(node2))
                        similarityMatrix.TryAdd(node2, new ConcurrentDictionary<T, double>());
                    similarityMatrix[node2].TryAdd(node1, 0.0);
                }
                // skip if nodes equal
                //if (EqualityComparer<T>.Default.Equals(node1, node2)) continue;
                
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

    public void PredictNewEdges(double treshhold)
    {
        foreach (var node1 in similarityMatrix.Keys)
        {
            foreach (var node2 in similarityMatrix[node1].Keys)
            {
                if (node2.CompareTo(node1) > 0 && similarityMatrix[node1][node2] > treshhold)
                {
                    AddNode(node1);
                    AddNode(node2);
                    AddUndirectedEdge(node1, node2);
                }
            }
            
        }
    }
    
    public ConcurrentDictionary<string, int> Compare(Graph<T> trueGraph)
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
                        confusionMatrix.AddOrUpdate("TP", 1, (key, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                    // Pred YES, True NO
                    else if ( (this.HasEdge(node1, node2) || this.HasEdge(node2, node1)) &&
                              (!trueGraph.HasEdge(node1, node2) && !trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("FP", 1, (key, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                    // Pred NO, True YES
                    else if ( (!this.HasEdge(node1, node2) && !this.HasEdge(node2, node1)) &&
                              (trueGraph.HasEdge(node1, node2) || trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("FN", 1, (key, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                    // Pred NO, True NO
                    else if ((!this.HasEdge(node1, node2) && !this.HasEdge(node2, node1)) &&
                             (!trueGraph.HasEdge(node1, node2) && !trueGraph.HasEdge(node2, node1)))
                    {
                        confusionMatrix.AddOrUpdate("TN", 1, (key, existingValue) =>
                        {
                            return existingValue + 1;
                        });
                    }
                }
                
            }
        });
        return confusionMatrix;
    }
    public void Merge(Graph<T> otherGraph)
    {
        // Merge Simplexes
        Parallel.ForEach(otherGraph.Simplexes, kvp =>
        {
            Simplexes.AddOrUpdate(kvp.Key, kvp.Value, (key, oldValue) => kvp.Value);
        });
        
        // Merge nodes and edges
        Parallel.ForEach(otherGraph.graph, kvp =>
        {
            if (!graph.ContainsKey(kvp.Key))
            {
                // If node from the other graph doesn't exists in current graph, then add him to current graph.
                graph.TryAdd(kvp.Key, new ConcurrentDictionary<T, int>());
            }

            foreach (var edge in kvp.Value)
            {
                // Add weight of the edge from other graph to the first one or update weight (old + new)
                graph[kvp.Key].AddOrUpdate(edge.Key, edge.Value, (key, oldValue) => edge.Value + oldValue);
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