namespace Project_Yahoo;

using System.Collections.Concurrent;

public class Graph
{
    public ConcurrentDictionary<int, ConcurrentDictionary<int, double>> Data { get; set; }
    public bool Directed { get; set; }
    public bool Weighted { get; set; }

    public List<int> GetNodes()
    {
        return Data.Keys.ToList();
    }
    public bool HasNode(int node)
    {
        return Data.ContainsKey(node);
    }
    public bool HasEdge(int fromNode, int toNode)
    {
        ConcurrentDictionary<int, double>? value;
        Data.TryGetValue(fromNode, out value);
        return value != null && value.ContainsKey(toNode);
    }
    public Graph()
    {
        this.Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();

        this.Directed = false;
        this.Weighted = true;
    }

    public Graph(Graph other)
    {
        //this.Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>(other.Data);
        this.Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
        foreach (var key in other.Data.Keys)
        {
            this.Data.TryAdd(key, new ConcurrentDictionary<int, double>(other.Data[key]));
        }
        
        this.Directed = other.Directed;
        this.Weighted = other.Weighted;
    }

    public Graph(bool directed = true, bool weighted = true)
    {
        this.Weighted = weighted;
        this.Directed = directed;
        this.Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>();
    }
    public int GetNumberOfNodes()
    { 
        return this.Data.Count;
    }
    
    public void Merge(Graph otherGraph)
    {
        //Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>(otherGraph.Data);
        //this.Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>(otherGraph.Data);
        //this.InEdges = new ConcurrentDictionary<int, ConcurrentDictionary<int, double>>(otherGraph.InEdges);
        foreach (var k in otherGraph.Data)
        {
            Data.TryAdd(k.Key, new ConcurrentDictionary<int, double>(k.Value));
        }

        // Merge nodes and edges
        /*Parallel.ForEach(otherGraph.Data, kvp =>
        {
            // If node from the other graph doesn't exists in current graph, then add him to current graph.
            //Data.GetOrAdd(kvp.Key, new ConcurrentDictionary<int, double>());
        });*/
        // Merge nodes and edges
        /*Parallel.ForEach(otherGraph.Data, kvp =>
        {
            if (!Data.ContainsKey(kvp.Key))
            {
                // If node from the other graph doesn't exists in current graph, then add him to current graph.
                Data.TryAdd(kvp.Key, new ConcurrentDictionary<int, double>());
            }

            foreach (var edge in kvp.Value)
            {
                // Add weight of the edge from other graph to the first one or update weight (old + new)
                Data[kvp.Key].AddOrUpdate(edge.Key, edge.Value, (key, oldValue) => edge.Value + oldValue);
            }
        });*/
    }
    
    public int GetNumberOfEdges()
    {
        if (!Directed)
        {
            // Divide by 2 for undirected graph
            return Data.Values.Sum(edges => edges.Count) / 2;
        }
        else
        {
            return Data.Values.Sum(edges => edges.Count);
        }
    }
    
    public void AddNode(int node)
    {
        this.Data.GetOrAdd(node, _ => new ConcurrentDictionary<int, double>());
    }
    
    public void AddUndirectedEdge(int nodeFrom, int nodeTo, double weight = 1.0)
    {
        // Check if the edge already exists. If yes, then add + 1 to its weight.
        if (this.Weighted)
        {
            // For weighted
            Data[nodeFrom].AddOrUpdate(nodeTo, weight, (_, oldValue) => oldValue + weight);
            Data[nodeTo].AddOrUpdate(nodeFrom, weight, (_, oldValue) => oldValue + weight);
        }
        else
        {
            // For not weighted
            Data[nodeFrom].GetOrAdd(nodeTo, weight);
            Data[nodeTo].GetOrAdd(nodeFrom, weight);
        }
    }

    public int GetDegree(int node)
    {
        return GetNeighbors(node).Keys.Count();
    }

    public double GetClusteringCoefficient(int node)
    {
        var neighbors = GetNeighbors(node).Keys.ToList();
        int numberOfNeighbors = neighbors.Count;
        if (numberOfNeighbors < 2)
        {
            return 0.0f; // Nodes with fewer than 2 neighbors have a clustering coefficient of 0.
        }

        var numberOfConnections = 0;
        // Check for connections between neighbors
        foreach (var neighbor1 in neighbors)
        {
            foreach (var neighbor2 in neighbors)
            {
                if (neighbor1 != neighbor2 && HasEdge(neighbor1, neighbor2))
                {
                    numberOfConnections++;
                }
            }
        }
        return (double)numberOfConnections / (numberOfNeighbors * (numberOfNeighbors - 1));
    }
    public ConcurrentDictionary<int, double> GetNeighbors(int node)
    {
        if (Data.TryGetValue(node, out var neighbors))
        {
            return neighbors;
        }
        else
        {
            return new ConcurrentDictionary<int, double>();
        }
    }
    
    private double Percentile(int[] sequence, double percentile)
    {
        Array.Sort(sequence);
        double realIndex = percentile * (sequence.Length - 1);
        int index = (int)realIndex;
        double frac = realIndex - index;
        return sequence[index] + frac * (sequence[index + 1] - sequence[index]);
    }

    public double ICSimulation(List<int> candidates, double p = 0.1)
    {
        ConcurrentDictionary<int, char> nodeStates = new ConcurrentDictionary<int, char>();
        // Initialize states of nodes
        foreach (var node in GetNodes())
        {
            nodeStates.TryAdd(node, '0');
        }
        /* States:
         '0' - Healthy
         '1' - Active
         '2' - Infected
         */
        // Initialize candidates to active
        foreach (var candidate in candidates)
        {
            nodeStates[candidate] = '1';
        }

        Random rnd = new Random();
        
        while (true)
        {
            List<int> activeNodes = nodeStates.Where(item => item.Value == '1').Select(item => item.Key).ToList();
            // If there arent any active vertices. Then end simulation.
            if (activeNodes.Count <= 0)
                break;
            // Select all healthy neighbours
            foreach (var activatedNode in activeNodes)
            {
                var neighbors = GetNeighbors(activatedNode).Keys.Where(node => nodeStates[node] == '0');
                foreach (var healthyNeighbor in neighbors)
                {
                    // Infect!
                    if (rnd.NextDouble() <= p)
                    {
                        nodeStates[healthyNeighbor] = '1';
                    }
                }
                // Change from active to infected
                nodeStates[activatedNode] = '2';
            }
        }

        var ratioOfInfection = nodeStates.Count(item => item.Value == '2') / (double)nodeStates.Keys.Count;
        //Console.WriteLine($"Ratio of infected nodes: {ratioOfInfection}");
        return ratioOfInfection;
    }
    public List<int> GetCandidates(int maxCandidates = 1, int numberOfNodes = 1000, double degreePercentile = 0.9, double clusteringCoefficientThreshold = 0.3)
    {
        Random random = new Random();
        
        double degreeThreshold1 = Percentile(GetNodes().Select(GetDegree).ToArray(), degreePercentile);
        //List<int> selectedVertexIds = Data.Keys.OrderBy(_ => random.Next()).Take(numberOfNodes).ToList();
        List<int> selectedVertexIds = Data.Keys.AsParallel().Where(node =>
        {
            return GetClusteringCoefficient(node) > clusteringCoefficientThreshold && GetDegree(node) > degreeThreshold1;
        }).ToList();
        
        Console.WriteLine($"Number of candidates (First round): {selectedVertexIds.Count}");
        int[] degrees = selectedVertexIds.Select(GetDegree).ToArray();
        
        double degreeThreshold2 = Percentile(degrees, degreePercentile);
        
        List<Tuple<int, double>> candidates = new List<Tuple<int,double>>();
        foreach (int vertex in selectedVertexIds)
        {
            double degree = GetDegree(vertex);
            if (degree >= degreeThreshold2)
            {
                double clusteringCoefficient = GetClusteringCoefficient(vertex);
                if (clusteringCoefficient <= 0.5)
                {
                    candidates.Add(new Tuple<int, double>(vertex, clusteringCoefficient));
                }
            }
        }

        candidates.Sort((v1, v2) => v1.Item2.CompareTo(v2.Item2));
        Console.WriteLine($"Number of candidates (Second round): {candidates.Count}");

        return candidates.Select(item => item.Item1).Take(maxCandidates).ToList();
    }
    public double CalculateDensity()
    {
        var numberOfEdges = GetNumberOfEdges();
        var numberOfNodes = (double)GetNumberOfNodes();

        if (!Directed)
            return (2 * numberOfEdges) / (numberOfNodes * (numberOfNodes - 1));
        else
            return (numberOfEdges) / (numberOfNodes * (numberOfNodes - 1));

    }
    public double CalculateAverageClusteringCoefficient()
    {
        double summedCC = 0.0f;
        foreach (var node in Data.Keys)
        {
            var neighbors = GetNeighbors(node).Keys.ToList();
            int numberOfNeighbors = neighbors.Count;
            if (numberOfNeighbors < 2)
            {
                continue; // Nodes with fewer than 2 neighbors have a clustering coefficient of 0.
            }

            var numberOfConnections = 0;
            
            // Check for connections between neighbors
            foreach (var neighbor1 in neighbors)
            {
                foreach (var neighbor2 in neighbors)
                {
                    if (neighbor1 != neighbor2 && HasEdge(neighbor1, neighbor2))
                    {
                        numberOfConnections++;
                    }
                }
            }
            summedCC += (double)numberOfConnections / (numberOfNeighbors * (numberOfNeighbors - 1));
        }

        return summedCC / Data.Keys.Count;
    }

    public void ExportToEdgeList(string outputFilePath)
    {
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine($"source,target,weight");
            // Write values
            foreach (var kvp1 in Data)
            {
                var node1 = kvp1.Key;
                foreach (var kvp2 in kvp1.Value)
                {
                    var node2 = kvp2.Key;
                    var weight = kvp2.Value;
                    // Write values
                    writer.WriteLine($"{node1},{node2},{weight}");
                }
                
            }
        }
    }
    public double CalculateAverageDegree()
    {
        //var sum = GetNodes().AsParallel().Sum(node => GetNeighbors(node).Count);
        var sum = Data.AsParallel().Sum(pair => pair.Value.Count) / 2;
        // Get average degree
        return sum / (double)GetNumberOfNodes();
    }

    
    public double CalculateAverageWeightedDegree()
    {
        var sum = Data.AsParallel().Sum(pair => pair.Value.Sum(values=> values.Value)) / 2;
        // Get average degree
        //return sum / (double)GetNumberOfNodes();
        return sum / (double)GetNumberOfNodes();
    }
    
    public ConcurrentDictionary<string, int> Compare(Graph trueGraph, List<int> chosenNodes)
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
        Parallel.ForEach(chosenNodes, node1 =>
        {
            foreach (var node2 in chosenNodes)
            {
                // Pred YES, True YES
                if ( (this.HasEdge(node1, node2) || this.HasEdge(node2, node1)) &&
                     (trueGraph.HasEdge(node1, node2) || trueGraph.HasEdge(node2, node1)))
                {
                    confusionMatrix["TP"] += 1;
                    //confusionMatrix.AddOrUpdate("TP", 1, (key, existingValue) => existingValue + 1);
                }
                // Pred YES, True NO
                else if ( (this.HasEdge(node1, node2) || this.HasEdge(node2, node1)) &&
                          (!trueGraph.HasEdge(node1, node2) && !trueGraph.HasEdge(node2, node1)))
                {
                    confusionMatrix["FP"] += 1;
                    //confusionMatrix.AddOrUpdate("FP", 1, (key, existingValue) => existingValue + 1);
                }
                // Pred NO, True YES
                else if ( (!this.HasEdge(node1, node2) && !this.HasEdge(node2, node1)) &&
                          (trueGraph.HasEdge(node1, node2) || trueGraph.HasEdge(node2, node1)))
                {
                    confusionMatrix["FN"] += 1;
                    //confusionMatrix.AddOrUpdate("FN", 1, (key, existingValue) => existingValue + 1);
                }
                // Pred NO, True NO
                else if ((!this.HasEdge(node1, node2) && !this.HasEdge(node2, node1)) &&
                         (!trueGraph.HasEdge(node1, node2) && !trueGraph.HasEdge(node2, node1)))
                {
                    confusionMatrix["TN"] += 1;
                    //confusionMatrix.AddOrUpdate("TN", 1, (key, existingValue) => existingValue + 1);
                }
            }
        });
        return confusionMatrix;
    }
    
    public KeyValuePair<int, int> CalculateMostCitedPatentId()
    {
        // Sort the concurrent dictionary by values
        /*if (InEdgeCount.Count <= 0)
            return new KeyValuePair<int, int>(-1, -1);*/
        
        //var mostCited = InEdgeCount.MaxBy(kv => kv.Value);
        // Find the key-value pair with the maximum count
        
        var maxByCount = Data
            .AsParallel()
            .Select(kvp => new { Key = kvp.Key, Count = kvp.Value.Count })
            .MaxBy(x => x.Count);

        if (maxByCount != null)
            return new KeyValuePair<int, int>(maxByCount.Key, maxByCount.Count);
        else
            return new KeyValuePair<int, int>(-1, -1);
    }
}