namespace DBLPAsMultilayer;

using System.Collections.Concurrent;

public class LayeredGraph
{
    public ConcurrentDictionary<int, GraphInLayer> graphs;
    private HashSet<int> actors;
    private bool weighted;

    public LayeredGraph(string filePath, bool weighted = false)
    {
        graphs = new ConcurrentDictionary<int, GraphInLayer>();
        actors = new HashSet<int>();
        this.weighted = weighted;
        
        Load(filePath);
    }

    public LayeredGraph(string numberOfNodesInSimplexesFilepath,
        string nodesInSimplexesFilepath, string yearsFilepath, int lengthOfTimeWindow, int maxYear, bool weighted = true)
    {
        graphs = new ConcurrentDictionary<int, GraphInLayer>();
        actors = new HashSet<int>();
        this.weighted = weighted;
        
        LoadDBLP(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow, maxYear);
    }

    private void LoadDBLP(string numberOfNodesInSimplexesFilepath,
        string nodesInSimplexesFilepath, string yearsFilepath, int lengthOfTimeWindow, int maxYear)
    {
        try
        { 
            // Open all 3 files
            using (StreamReader timeReader = new StreamReader(yearsFilepath))
            using (StreamReader numberOfNodesReader = new StreamReader(numberOfNodesInSimplexesFilepath))
            using (StreamReader nodesReader = new StreamReader(nodesInSimplexesFilepath))
            {
                int numberOfSimplex = 1;
                // Check if you are not on the end of the file of 2 files.
                while (!timeReader.EndOfStream && !numberOfNodesReader.EndOfStream)
                {
                    string? lineTime = timeReader.ReadLine();
                    string? lineNumberOfVertexes = numberOfNodesReader.ReadLine();

                    if (lineTime == null || lineNumberOfVertexes == null) continue;
                    
                    int year = int.Parse(lineTime.Replace("\n", ""));
                    int numberOfVertexes = int.Parse(lineNumberOfVertexes.Replace("\n", ""));
                    if (year > maxYear)
                    {
                        for (int i = 0; i < numberOfVertexes; i++)
                        {
                            nodesReader.ReadLine();
                        }
                        continue;
                    }
                    List<int> nodesInSimplex = new List<int>();
                    // Read numberOfVertexes line from file
                    for (int i = 0; i < numberOfVertexes; i++)
                    {
                        string lineNode = nodesReader.ReadLine();
                        if (lineNode == null) continue;
                        nodesInSimplex.Add(int.Parse(lineNode.Replace("\n", "")));
                    }
                    
                    // Add connection for each node in simplex with another node in simplex
                    for (int i = 0; i < nodesInSimplex.Count; i++)
                    {
                        if(!HasLayer(year))
                            InitLayer(year);
                
                        AddActor(nodesInSimplex[i]);
                        AddNode(year,nodesInSimplex[i]);
                        
                        for (int j = i + 1; j < nodesInSimplex.Count; j++)
                        {
                            AddActor(nodesInSimplex[j]);
                            AddNode(year, nodesInSimplex[j]);
                            AddUndirectedEdge(year, nodesInSimplex[i], nodesInSimplex[j]);
                        }
                    }

                    this.graphs[year].Simplexes.TryAdd(numberOfSimplex, nodesInSimplex);
                    numberOfSimplex++;
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("The file does not exist.");
        }
        catch (IOException e)
        {
            Console.WriteLine($"An error occurred while reading the file: {e.Message}");
        }
        
        // Create a sorted list of keys
        List<int> sortedKeys = this.graphs.Keys.ToList();
        sortedKeys.Sort(); // Sort the list of keys
        ConcurrentDictionary<int, GraphInLayer> graphForEachYearSorted = new ConcurrentDictionary<int, GraphInLayer>();
        // Iterate over the ConcurrentDictionary based on sorted keys and take only until max point.
        foreach (int year in sortedKeys)
        {
            if (year > maxYear) break;
            
            GraphInLayer value;
            if (this.graphs.TryGetValue(year, out value))
            {
                graphForEachYearSorted.TryAdd(year, value);
            }
        }

        this.graphs = graphForEachYearSorted;
        
        
        /*foreach (var kvp in graphForEachYearSorted)
        {
            Console.WriteLine($"Year {kvp.Key}: {kvp.Value.NodesCount} nodes, {kvp.Value.EdgeCount} edges.");
        }*/
        
        // Make time steps
        ConcurrentDictionary<int, GraphInLayer> graphForEachTimeStep = new ConcurrentDictionary<int, GraphInLayer>();
        for (int i = 0; i < graphForEachYearSorted.Keys.Count; i+=lengthOfTimeWindow)
        {
            var yearsOfTimeStep = graphForEachYearSorted.Keys.Skip(i).Take(lengthOfTimeWindow).ToList();
            int key = yearsOfTimeStep[0];
            var actualGraph = graphForEachYearSorted[yearsOfTimeStep[0]];
            for (int j = 1; j < yearsOfTimeStep.Count; j++)
            {
                var nextGraph = graphForEachYearSorted[yearsOfTimeStep[j]];
                actualGraph.Merge(nextGraph);
            }
            graphForEachTimeStep.TryAdd(key, actualGraph);
        }
        this.graphs = graphForEachTimeStep;
        
        sortedKeys = this.graphs.Keys.ToList();

        for (int i = 0; i < sortedKeys.Count; i++)
        {
            Console.WriteLine($"Year {sortedKeys[i]}: {graphForEachTimeStep[sortedKeys[i]].NodesCount} nodes, {graphForEachTimeStep[sortedKeys[i]].EdgeCount} edges, {graphForEachTimeStep[sortedKeys[i]].Simplexes.Keys.Count} simplexes");
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
            graphs.TryAdd(layer, new GraphInLayer(this.weighted));
        }
    }
    private bool HasLayer(int layer)
    {
        return graphs.ContainsKey(layer);
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

    public double GetConnectiveRedundancy(int actor, List<int> layers)
    {
        return 1 - ((double)GetNeighborhoodCentrality(actor, layers) / GetDegreeCentrality(actor, layers));
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
}