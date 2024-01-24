using System.Collections.Concurrent;

namespace MultilayeredGraphs;

public class LayeredGraph
{
    public ConcurrentDictionary<int, Graph> graphs;
    private HashSet<int> actors;
    private bool weighted;

    public LayeredGraph(string filePath, bool weighted = false)
    {
        graphs = new ConcurrentDictionary<int, Graph>();
        actors = new HashSet<int>();
        this.weighted = weighted;
        
        Load(filePath);
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
                int layerId = int.Parse(indices[0]);
                int idFrom = int.Parse(indices[1]);
                int idTo = int.Parse(indices[2]);
                float weight = float.Parse(indices[3]);


                if(!HasLayer(layerId))
                    InitLayer(layerId);
                
                AddActor(idTo);
                AddActor(idFrom);
                AddNode(layerId,idFrom);
                AddNode(layerId, idTo);
                AddUndirectedEdge(layerId, idFrom, idTo, weight);
            }
        }
    }

    private void InitLayer(int layer)
    {
        if (!graphs.ContainsKey(layer))
        {
            graphs.TryAdd(layer, new Graph(this.weighted));
        }
    }
    private bool HasLayer(int layer)
    {
        return graphs.ContainsKey(layer);
    }

    private List<Tuple<int, int>> GetLayersForActor(int actor, List<int> layers)
    {
        return layers
            .Where(layer => graphs[layer].HasNode(actor))
            .Select(layer => Tuple.Create(actor, layer))
            .ToList();
    }
    private HashSet<int> GetActorsForLayers(List<int> layers)
    {
        HashSet<int> actors = new HashSet<int>();
        foreach (var layer in layers)
        {
            actors.UnionWith(graphs[layer].Nodes);
        }

        return actors;
    }
    
    private void AddActor(int actor)
    {
        this.actors.Add(actor);
    }
    private void AddNode(int layer, int node)
    {
        graphs[layer].AddNode(node);
    }

    private void AddUndirectedEdge(int layer, int nodeFrom, int nodeTo, double weight = 1.0)
    {
        graphs[layer].AddUndirectedEdge(nodeFrom, nodeTo, weight);
    }

    public List<int> GetIdOfLayers()
    {
        return graphs.Keys.ToList();
    }
    
    // Degree based measures
    public int GetDegreeCentrality(int actor, List<int> layers)
    {
        return layers.Sum(layerId => graphs[layerId].GetLocalDegree(actor));
    }

    public double GetDegreeCentrality(List<int> layers)
    {
        var _sum = this.actors.Sum(actor => GetDegreeCentrality(actor, layers));
        return (double)_sum / this.actors.Count;
    }

    public double GetDegreeDeviation(int actor, List<int> layers)
    {
        var degreeCentralityOfAllLayers = GetDegreeCentrality(actor, GetIdOfLayers());
        var countOfLayers = GetIdOfLayers().Count();
        var _sum = 0.0;
        foreach (var layerId in layers)
        {
            var degreeCentralityOfSingleLayer = GetDegreeCentrality(actor, new List<int> { layerId });
            _sum += Math.Pow(degreeCentralityOfSingleLayer - (degreeCentralityOfAllLayers / (double)countOfLayers), 2);
        }

        _sum = _sum / countOfLayers;
        _sum = Math.Sqrt(_sum);
        return _sum;
    }
    public double GetDegreeDeviation(List<int> layers)
    {
        var _sum = this.actors.Sum(actor => GetDegreeDeviation(actor, layers));
        return (double)_sum / this.actors.Count;
    }
    // Neighborhood based measures
    public HashSet<int> GetNeighbors(int actor, List<int> layers)
    {
        HashSet<int> neighbors = new HashSet<int>();
        foreach (var layerId in layers)
        {
            neighbors.UnionWith(graphs[layerId].GetNeighbors(actor));
        }
        return neighbors;
    }

    public int GetNeighborhoodCentrality(int actor, List<int> layers)
    {
        return GetNeighbors(actor, layers).Count;
    }
    
    public double GetNeighborhoodCentrality(List<int> layers)
    {
        var _sum = this.actors.Sum(actor => GetNeighborhoodCentrality(actor, layers));
        return (double)_sum / this.actors.Count;
    }
    
    public double GetConnectiveRedundancy(int actor, List<int> layers)
    {
        return 1 - ((double)GetNeighborhoodCentrality(actor, layers) / GetDegreeCentrality(actor, layers));
    }
    public double GetConnectiveRedundancy(List<int> layers)
    {
        var _sum = this.actors.Sum(actor => GetConnectiveRedundancy(actor, layers));
        return (double)_sum / this.actors.Count;
    }

    public int GetExclusiveNeighborhoodCentrality(int actor, List<int> layers)
    {
        // neighbors(a, L)
        var neighbors = GetNeighbors(actor, layers);
        // neighbors(a, L\L)
        var neighborsInLayersWithoutCurrent = GetNeighbors(actor, GetIdOfLayers().Where(layer => !layers.Contains(layer)).ToList());
        // neighbors(a, L) \ neighbors(a, L\L)
        var xneighboors = neighbors.Where(neighbor => !neighborsInLayersWithoutCurrent.Contains(neighbor)).ToList();
        
        return xneighboors.Count;
    }
    public double GetExclusiveNeighborhoodCentrality(List<int> layers)
    {
        var _sum = this.actors.Sum(actor => GetExclusiveNeighborhoodCentrality(actor, layers));
        return (double)_sum / this.actors.Count;
    }
    
    public double GetRelevance(int actor, List<int> layers)
    {
        return GetNeighborhoodCentrality(actor, layers) / (double)GetNeighborhoodCentrality(actor, GetIdOfLayers());
    }

    public Dictionary<int, Dictionary<int, double>> GetRelevance()
    {
        Dictionary<int, Dictionary<int, double>> tableOfRelevance = new Dictionary<int, Dictionary<int, double>>();

        foreach (var actor in actors)
        {
            tableOfRelevance[actor] = new Dictionary<int, double>();
            foreach (var layer in GetIdOfLayers())
            {
                tableOfRelevance[actor][layer] = GetRelevance(actor, new List<int>() { layer });
            }
        }

        return tableOfRelevance;
    }
    
    public double GetExclusiveRelevance(int actor, List<int> layers)
    {
        return GetExclusiveNeighborhoodCentrality(actor, layers) / (double)GetNeighborhoodCentrality(actor, GetIdOfLayers());
    }
    
    public Dictionary<int, Dictionary<int, double>> GetExclusiveRelevance()
    {
        Dictionary<int, Dictionary<int, double>> tableOfRelevance = new Dictionary<int, Dictionary<int, double>>();

        foreach (var actor in actors)
        {
            tableOfRelevance[actor] = new Dictionary<int, double>();
            foreach (var layer in GetIdOfLayers())
            {
                tableOfRelevance[actor][layer] = GetExclusiveRelevance(actor, new List<int>() { layer });
            }
        }

        return tableOfRelevance;
    }

    public void ExportTable(Dictionary<int, Dictionary<int, double>> table, string fileName = "output.csv")
    {
        string headerLine = "User;";
        using (StreamWriter writer = new StreamWriter($"Outputs/{fileName}"))
        {
            // Write header
            writer.Write("User;");
            foreach (var layerId in GetIdOfLayers())
            {
                writer.Write($"{layerId};");
            }
            writer.Write("\n");
            
            // Write rows for each actor
            foreach (var row in table)
            {
                // Actor
                writer.Write($"{row.Key};");
                // Values for each layer
                foreach (var layerId in GetIdOfLayers())
                {
                    if (row.Value.ContainsKey(layerId))
                        writer.Write($"{Math.Round(row.Value[layerId], 2)};");
                    else
                        writer.Write($"None;");
                }
                writer.Write("\n");
            }
        }
        
    }

    private int GetOccupationCentralityUsingUniformRandomWalk(int actor, List<int> layers, int numberOfSteps)
    {
        int occupationCentrality = 0;
        // Initialize start node and start layer.
        Random random = new Random();
        //var actorsOfLayers = GetActorsForLayers(layers);
        //var currentNode = actorsOfLayers.ToList()[random.Next(actorsOfLayers.Count)];
        var currentNode = actor;
        var layersOfCurrentNode = GetLayersForActor(currentNode, layers).Select(item => item.Item2).ToList();
        var currentLayer = layersOfCurrentNode[random.Next(layersOfCurrentNode.Count)];
        if (currentNode == actor)
            occupationCentrality += 1;
        
        // Random walk for defined number steps.
        for (int step = 0; step < numberOfSteps; step++)
        {
            // Get neighbors of actor in current layer
            var neighbors = GetNeighbors(currentNode, new List<int>(){currentLayer})
                .Select(neighbor => Tuple.Create(neighbor, currentLayer))
                .ToList();
            // Get all nodes in other layers of current actor.
            var layersWithThisActor = GetLayersForActor(currentNode, layers);
            // Concat all options.
            var possibleNextNodes = neighbors.Concat(layersWithThisActor).ToList();
            // Randomly choose next node and next layer
            var nextNodeAndLayer = possibleNextNodes[random.Next(possibleNextNodes.Count)];
            
            currentNode = nextNodeAndLayer.Item1;
            currentLayer = nextNodeAndLayer.Item2;
            
            // Get occupation centrality
            if (currentNode == actor)
                occupationCentrality += 1;
        }

        return occupationCentrality;
    }
    public double GetOccupationCentralityUsingUniformRandomWalk(int actor, List<int> layers, int numberOfSteps, int numberOfWalks)
    {
        int totalOccupationCentrality = 0;
        for (int i = 0; i < numberOfWalks; i++)
        {
            totalOccupationCentrality += GetOccupationCentralityUsingUniformRandomWalk(actor, layers, numberOfSteps);
        }

        return totalOccupationCentrality / (double)(numberOfWalks);
    }
    
    public double GetOccupationCentralityUsingUniformRandomWalk(List<int> layers, int numberOfSteps, int numberOfWalks)
    {
        var sumOfOccCentrality = 0.0;
        foreach (var actor in actors)
        {
            int totalOccupationCentrality = 0;
            for (int i = 0; i < numberOfWalks; i++)
            {
                totalOccupationCentrality += GetOccupationCentralityUsingUniformRandomWalk(actor, layers, numberOfSteps);
            }
            sumOfOccCentrality += (totalOccupationCentrality / (double)(numberOfWalks));
        }
        
        return sumOfOccCentrality / (actors.Count);
    }

    public Graph GetUnweightedFlattenedGraph(List<int> layers)
    {
        Graph flattenedGraph = new Graph(weighted);
        // Get all actors of layers
        var actorsOfLayers = GetActorsForLayers(layers);
        // Add All actors as nodes in flattened graph
        foreach (var actor in actorsOfLayers)
        {
            flattenedGraph.AddNode(actor);
            // Add all edges from all layers
            foreach (var layer in layers)
            {
                var neigbors = graphs[layer].GetNeighbors(actor);
                foreach (var neighbor in neigbors)
                {
                    flattenedGraph.AddNode(neighbor);
                    flattenedGraph.AddUndirectedEdge(actor, neighbor);
                }
            }
        }

        

        return flattenedGraph;
    }
}