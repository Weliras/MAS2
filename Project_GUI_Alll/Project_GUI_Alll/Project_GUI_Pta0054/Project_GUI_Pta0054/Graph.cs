using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;

namespace Project_GUI_Pta0054;

public class Graph
{
    public ConcurrentDictionary<int, ConcurrentDictionary<int, bool>> Data = new();
    public ConcurrentDictionary<int, int> Degrees = new();
    private Random random = new Random();

    public List<int> Nodes => Data.Keys.ToList();
    // Define an event to notify about progress
    public event Action<double> ProgressChangedValue;
    public event Action<string> ProgressChangedName;
    
    public void AddNode(int node)
    {
        Data.TryAdd(node, new ConcurrentDictionary<int, bool>());
    }

    public bool HasNode(int node)
    {
        return Data.ContainsKey(node);
    }

    public bool HasEdge(int node1, int node2)
    {
        return Data[node1].ContainsKey(node2);
    }

    public void CalculateDegree(int node)
    {
        Degrees.AddOrUpdate(node, _ => Data[node].Keys.Count, (_, _) => Data[node].Keys.Count);
    }
    public int GetDegree(int node)
    {
        return Degrees[node];
    }
    public List<int> GetNeighbours(int node)
    {
        return Data[node].Keys.ToList();
    }
    protected virtual void OnProgressChangedValue(double progress)
    {
        // Fire the event if there are subscribers
        ProgressChangedValue?.Invoke(progress);
    }
    protected virtual void OnProgressChangedName(string progress)
    {
        // Fire the event if there are subscribers
        ProgressChangedName?.Invoke(progress);
    }
    
    public void AddUndirectedEdge(int node1, int node2)
    {
        if (HasNode(node1) && HasNode(node2))
        {
            Data[node1].TryAdd(node2, true);
            Data[node2].TryAdd(node1, true);
        }
    }

    public void RemoveUndirectedEdge(int node1, int node2)
    {
        if (HasNode(node1) && HasNode(node2))
        {
            Data[node1].Remove(node2, out _);
            Data[node2].Remove(node1, out _);
        }
    }

    public void CreateBarabasiAlbertModel(int n, int m, int d)
    {
        void InitializeCompleteGraph(int m)
        {
            // Create m nodes
            for (int i = 0; i < m; i++)
            {
                AddNode(i);
            }
            // Create complete initial graph
            foreach (var node1 in Nodes)
            {
                foreach (var node2 in Nodes)
                {
                    if (node1 != node2)
                    {
                        AddUndirectedEdge(node1, node2);
                    }
                }
            }
            // Compute initial degrees
            foreach (var node in Nodes)
            {
                CalculateDegree(node);
            }
        }
        
        void PreferentialAttachment(int n, int m, int d)
        {
            // Start adding nodes
            for (int i = m; i < n; i++)
            {
                // update progress bar
                OnProgressChangedValue((i / (double)n * 100) );
                // Get original nodes
                var originalNodes = new List<int>(Nodes);
                // Get sum of all degrees.
                var sumOfAllDegrees = Degrees.Sum(item => item.Value) == 0 ? 1 : Degrees.Sum(item => item.Value);
                // Add new node
                AddNode(i);
                Degrees.TryAdd(i, 0);

                int countOfConnections = 0;
                // Calculate probability of edge with all existing nodes
                foreach (var node in originalNodes)
                {
                    Degrees.TryGetValue(node, out d);
                    double p =  d / (double)sumOfAllDegrees;

                    if (random.NextDouble() < p)
                    {
                        Degrees[node]++;
                        Degrees[i]++;
                        AddUndirectedEdge(node, i);
                        // Check if we already added enough edges.
                        countOfConnections++;
                        if (countOfConnections >= d)
                            break;
                    }
                }
                // If didnt add edge, then add it randomly
                while (countOfConnections < d)
                {
                    int randomNode = random.Next(originalNodes.Count);
                    Degrees[randomNode]++;
                    Degrees[i]++;
                    AddUndirectedEdge(randomNode, i);
                    countOfConnections++;
                }
            }
        }
        Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, bool>>();
        Degrees = new ConcurrentDictionary<int, int>();
        
        InitializeCompleteGraph(m);
        OnProgressChangedName("Attaching nodes...");
        PreferentialAttachment(n, m, d);
        OnProgressChangedName("Graph according to a Barabási-Albert model was created.");
    }
    public void CreateWattsStrogatzModel(int n, int k, double p)
    {
        void GenerateLatticeGraph(int n, int k)
        {
            // for each node
            for (int i = 0; i < n; i++)
            {
                AddNode(i);
                // update progress bar
                OnProgressChangedValue((i / (double)n * 100) / 2);
            
                // for each neighbour
                for (int j = 1; j <= k / 2; j++)
                {
                    int neighbour = (i + j) % n;
                    AddNode(neighbour);
                    AddUndirectedEdge(i, neighbour);
                }
            }
        }
        void RewireEdges(int node, int k, double p)
        {
        
            var neighbours = GetNeighbours(node);
        
            foreach (var neighbour in neighbours)
            {
                if (random.NextDouble() < p)
                {
                    int newNeigbour;
                    do
                    {
                        newNeigbour = random.Next(Data.Count);
                    } while (newNeigbour == node || neighbours.Contains(newNeigbour) || HasEdge(node, newNeigbour));
                
                    // Remove old link
                    RemoveUndirectedEdge(node, neighbour);
                    // Add new link
                    AddUndirectedEdge(node, newNeigbour);
                }
            }
        }
        Data = new ConcurrentDictionary<int, ConcurrentDictionary<int, bool>>();
        OnProgressChangedName("Generating lattice...");
        GenerateLatticeGraph(n, k);
        
        OnProgressChangedName("Rewiring edges...");
        int i = 0;
        foreach (var node in Nodes)
        {
            // update progress bar
            OnProgressChangedValue(50 +(i / (double)Nodes.Count * 100) / 2);
            RewireEdges(node, k, p);
            i++;
        }
        
        OnProgressChangedName("Graph according to a Watts-Strogatz model was created.");
    }
    public void ExportToEdgeList(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write header
            writer.WriteLine($"source,target");
            // Write values
            foreach (var kvp1 in Data)
            {
                var node1 = kvp1.Key;
                foreach (var kvp2 in kvp1.Value)
                {
                    var node2 = kvp2.Key;
                    // Write values
                    writer.WriteLine($"{node1},{node2}");
                }
                
            }
        }
    }
    
    public void ExportToAdjacencyList(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write values
            foreach (var kvp1 in Data)
            {
                var node1 = kvp1.Key;
                writer.Write($"{node1}");
                foreach (var kvp2 in kvp1.Value)
                {
                    var node2 = kvp2.Key;
                    // Write values
                    writer.Write($",{node2}");
                }
                writer.Write("\n");
            }
        }
    }
    
    private int GetOccupationCentrality(int node, int numberOfSteps)
    {
        int occupationCentrality = 0;
        // Initialize start node and start layer.
        Random random = new Random();
        //var actorsOfLayers = GetActorsForLayers(layers);
        //var currentNode = actorsOfLayers.ToList()[random.Next(actorsOfLayers.Count)];
        var currentNode = node;
        /*if (currentNode == actor)
            occupationCentrality += 1;*/
        
        // Random walk for defined number steps.
        for (int step = 0; step < numberOfSteps; step++)
        {
            // Get neighbors of actor in current layer
            var neighbors = GetNeighbours(currentNode);
            // Randomly choose next node and next layer
            var nextNode = neighbors[random.Next(neighbors.Count)];
            
            currentNode = nextNode;
            
            // Get occupation centrality
            if (currentNode == node)
                occupationCentrality += 1;
        }

        return occupationCentrality;
    }
    
    public double GetOccupationCentrality(int node, int numberOfSteps, int numberOfWalks)
    {
        int totalOccupationCentrality = 0;
        for (int i = 0; i < numberOfWalks; i++)
        {
            totalOccupationCentrality += GetOccupationCentrality(node, numberOfSteps);
        }
        return totalOccupationCentrality / (double)(numberOfWalks);
    }

    public double GetAverageDegreeCentrality()
    {
        var degrees = 0;
        Parallel.ForEach(Nodes, node =>
        {
            degrees += GetDegree(node);
        });
        return degrees / (double)Nodes.Count;
    }
    public double GetAverageOccupationCentrality(int numberOfSteps, int numberOfWalks)
    {
        OnProgressChangedName("Calculating average occupation centrality...");

        var sumOfOccCentrality = 0.0;
        var nodeCount = Nodes.Count;
        Parallel.ForEach(Nodes, (node, loopState, index) =>
        {
            OnProgressChangedValue((index / (double)nodeCount * 100));
            sumOfOccCentrality += GetOccupationCentrality(node, numberOfSteps, numberOfWalks);
        });
        /*foreach (var node in Nodes)
        {
            
        }*/
        OnProgressChangedValue(100);

        
        return sumOfOccCentrality / (Nodes.Count);
    }
}
    