using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace Project_Yahoo;



public class MultiLayerGraph
{
    public ConcurrentDictionary<int, Graph> GraphsInLayers { get; set; }
    public HashSet<int> Actors { get; set; }

    // Return Hashset of layer ids.
    public HashSet<int> GetLayers()
    {
        return this.GraphsInLayers.Keys.Order().ToHashSet();
    }

    public MultiLayerGraph()
    {
        this.GraphsInLayers = new ConcurrentDictionary<int, Graph>();
        this.Actors = new HashSet<int>();
    }
    public MultiLayerGraph(ConcurrentDictionary<int, Graph> graphs, HashSet<int> actors)
    {
        this.GraphsInLayers = new ConcurrentDictionary<int, Graph>(graphs);
        this.Actors = new HashSet<int>();
    }
    
    public MultiLayerGraph(string pathToDirectory)
    {
        this.GraphsInLayers = new ConcurrentDictionary<int, Graph>();
        this.Actors = new HashSet<int>();
        
        // Load layers from files
        Load(pathToDirectory);
    }

    public Graph GetGraphForLayer(int layerId)
    {
        return this.GraphsInLayers[layerId];
    }

    public int GetNumberOfActors()
    {
        return this.Actors.Count;
    }
    
    private void Load(string pathToDirectory)
    {
        void LoadEdges(string pathToFile)
        {
            using (StreamReader reader = new StreamReader(pathToFile))
            {
                int step = 100_000;
                string? line;
                int timePeriod = 1;
                // Init first layer
                InitLayer(timePeriod);
                // Process lines
                while ((line = reader.ReadLine()) != null)
                {
                    // Process the line
                    var stringSplitted = line.Replace("\n", "").Split(' ');
                    int node1 = int.Parse(stringSplitted[0]);
                    int node2 = int.Parse(stringSplitted[1]);
                    double weight = double.Parse(stringSplitted[2]);
                    int timestamp = int.Parse(stringSplitted[3]);
                    
                    if (timestamp >= step * timePeriod)
                    {
                        timePeriod++;
                        /*if (timePeriod > 2)
                            break;*/
                        InitLayer(timePeriod);
                        GetGraphForLayer(timePeriod).Merge(GetGraphForLayer(timePeriod-1));
                    }
                    
                    Actors.Add(node1);
                    Actors.Add(node2);
                    
                    AddNode(timePeriod, node1);
                    AddNode(timePeriod, node2);

                    AddUndirectedEdge(timePeriod, node1, node2, weight);
                }
            }
        }
        
        // construct paths to expected files
        string filePathAdjacencyList = Path.Combine(pathToDirectory, "ia-yahoo-messages.mtx");
        // Timer for time measurement
        // Load Actors
        Stopwatch sw = new Stopwatch();
        /*
        LoadActors(filePathNodeDescriptions);
        sw.Stop();
        Console.WriteLine($"Elapsed time in LoadActors: {sw.Elapsed}");
        */
        
        sw.Restart();
        LoadEdges(filePathAdjacencyList);
        sw.Stop();
        Console.WriteLine($"Elapsed time in LoadEdges: {sw.Elapsed}");

    }

    private void AddActor(int actorId, string grantYear)
    {
        this.Actors.Add(actorId);
    }

    private void InitLayer(int layerId)
    {
        // Check if layer already exists
        if (!this.GraphsInLayers.ContainsKey(layerId))
        {
            this.GraphsInLayers.TryAdd(layerId, new Graph());
        }
    }

    private void AddNode(int layerId, int actorId)
    {
        this.GraphsInLayers[layerId].AddNode(actorId);

    }

    private void AddUndirectedEdge(int layer, int nodeFrom, int nodeTo, double weight = 1.0)
    {
        GraphsInLayers[layer].AddUndirectedEdge(nodeFrom, nodeTo, weight);
    }
    

    public void LogAverageDegreeForEachLayer(string outputFilePath)
    {
        ConcurrentDictionary<int, Tuple<double, double>> degreeForLayer = new ConcurrentDictionary<int, Tuple<double, double>>();
        
        Parallel.ForEach(GetLayers(), layer =>
        {
            degreeForLayer.GetOrAdd(layer, new Tuple<double, double>(GraphsInLayers[layer].CalculateAverageDegree(), GraphsInLayers[layer].CalculateAverageWeightedDegree()));
        });
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine($"Year;AverageDegree;AverageWeightedDegree");
            foreach (var kvp in degreeForLayer)
            {
                // Write values
                writer.WriteLine($"{kvp.Key};{kvp.Value.Item1};{kvp.Value.Item2}");
            }
        }
    }
    
    public void LogNodesAndEdgesForEachLayer(string outputFilePath)
    {
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine($"Year;Nodes;Edges");
            // Write values
            foreach (var layerId in GetLayers())
            {
                var patentsInYear = GetGraphForLayer(layerId);
                // Write values
                writer.WriteLine($"{layerId};{patentsInYear.GetNumberOfNodes()};{patentsInYear.GetNumberOfEdges()}");
            }
        }
    }
    public void LogAverageClusteringCoefficient(string outputFilePath)
    {
        ConcurrentDictionary<int, double> CCForLayer = new ConcurrentDictionary<int, double>();

        Parallel.ForEach(GetLayers(), layer =>
        {
            CCForLayer.GetOrAdd(layer, GraphsInLayers[layer].CalculateAverageClusteringCoefficient());
        });
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine($"Year;AverageClusteringCoefficient");
            // Write values
            foreach (var kvp in CCForLayer)
            {
                // Write values
                writer.WriteLine($"{kvp.Key};{kvp.Value}");
            }
        }
    }
    
    public void LogDensity(string outputFilePath)
    {
        ConcurrentDictionary<int, double> value = new ConcurrentDictionary<int, double>();

        Parallel.ForEach(GetLayers(), layer =>
        {
            value.GetOrAdd(layer, GraphsInLayers[layer].CalculateDensity());
        });
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine($"Year;Density");
            // Write values
            foreach (var kvp in value)
            {
                // Write values
                writer.WriteLine($"{kvp.Key};{kvp.Value}");
            }
        }
    }
    
    public void LogNodeWithMaxDegree(string outputFilePath)
    {
        ConcurrentDictionary<int, KeyValuePair<int, int>> mostCitedPatentForLayer =
            new ConcurrentDictionary<int, KeyValuePair<int, int>>();

        Parallel.ForEach(GetLayers(), layer =>
        {
            mostCitedPatentForLayer.GetOrAdd(layer, GraphsInLayers[layer].CalculateMostCitedPatentId());
        });
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine($"Year;NodeWithMostDegree;Degree");
            // Write values
            foreach (var kvp in mostCitedPatentForLayer)
            {
                // Write values
                writer.WriteLine($"{kvp.Key};{kvp.Value.Key};{kvp.Value.Value}");
            }
        }
    }
}