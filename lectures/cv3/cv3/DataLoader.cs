namespace cv2;

using System.Collections.Concurrent;

public class DataLoader
{
    public static Graph<int> LoadNetworkInt(string filePath)
    {
        Console.WriteLine($"Loading network: {filePath}");
        Graph<int> network = new Graph<int>();
        //ConcurrentDictionary<int, ConcurrentDictionary<int, bool>> dictionaryOfKeys = new ConcurrentDictionary<int, ConcurrentDictionary<int, bool>>();
        try
        { 
            // Open the file for reading using StreamReader
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read and display the contents line by line
                while (reader.ReadLine() is { } line)
                {
                    if (line.StartsWith("#"))
                        continue;
                    var parts = line.Replace("\n", "").Split();
                    int idFrom = int.Parse(parts[0]);
                    int idTo = int.Parse(parts[1]);
                    //Console.WriteLine($"{idFrom}->{idTo}");
                    
                    // Add edge
                    network.AddUndirectedEdge(idFrom, idTo);
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

        return network;
    }

    public static ConcurrentDictionary<int, Graph<int>> LoadDBLPGraph(string numberOfNodesInSimplexesFilepath,
        string nodesInSimplexesFilepath, string yearsFilepath, int lengthOfTimeWindow, int maxYear)
    {
        ConcurrentDictionary<int, Graph<int>> graphForEachYear = new ConcurrentDictionary<int, Graph<int>>();
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
                    
                    // Add graph if doesnt exist for year
                    graphForEachYear.GetOrAdd(year, _ => new Graph<int>());
                    // Add connection for each node in simplex with another node in simplex
                    for (int i = 0; i < nodesInSimplex.Count; i++)
                    {
                        graphForEachYear[year].AddNode(nodesInSimplex[i]);
                        for (int j = i + 1; j < nodesInSimplex.Count; j++)
                        {
                            graphForEachYear[year].AddNode(nodesInSimplex[j]);
                            graphForEachYear[year].AddUndirectedEdge(nodesInSimplex[i], nodesInSimplex[j]);
                        }
                    }

                    graphForEachYear[year].Simplexes.TryAdd(numberOfSimplex, nodesInSimplex);
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
        List<int> sortedKeys = graphForEachYear.Keys.ToList();
        sortedKeys.Sort(); // Sort the list of keys
        ConcurrentDictionary<int, Graph<int>> graphForEachYearSorted = new ConcurrentDictionary<int, Graph<int>>();
        // Iterate over the ConcurrentDictionary based on sorted keys and take only until max point.
        foreach (int year in sortedKeys)
        {
            if (year > maxYear) break;
            
            Graph<int> value;
            if (graphForEachYear.TryGetValue(year, out value))
            {
                graphForEachYearSorted.TryAdd(year, value);
            }
        }
        graphForEachYear = null;
        
        /*foreach (var kvp in graphForEachYearSorted)
        {
            Console.WriteLine($"Year {kvp.Key}: {kvp.Value.NodesCount} nodes, {kvp.Value.EdgeCount} edges.");
        }*/
        
        // Make time steps
        ConcurrentDictionary<int, Graph<int>> graphForEachTimeStep = new ConcurrentDictionary<int, Graph<int>>();
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

        graphForEachYearSorted = null;
        
        sortedKeys = graphForEachTimeStep.Keys.ToList();
        // additive
        Console.WriteLine($"Year {sortedKeys[0]}: {graphForEachTimeStep[sortedKeys[0]].NodesCount} nodes, {graphForEachTimeStep[sortedKeys[0]].EdgeCount} edges, {graphForEachTimeStep[sortedKeys[0]].Simplexes.Keys.Count} simplexes");

        for (int i = 1; i < sortedKeys.Count; i++)
        {
            // merge actual network with previous one
            graphForEachTimeStep[sortedKeys[i]].Merge(graphForEachTimeStep[sortedKeys[i - 1]]);
            Console.WriteLine($"Year {sortedKeys[i]}: {graphForEachTimeStep[sortedKeys[i]].NodesCount} nodes, {graphForEachTimeStep[sortedKeys[i]].EdgeCount} edges, {graphForEachTimeStep[sortedKeys[i]].Simplexes.Keys.Count} simplexes");
        }
        // Create a sorted list of keys
        //var graphForEachTimeStepSorted = new ConcurrentDictionary<int, Graph<int>>(graphForEachTimeStep.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value));
        //graphForEachTimeStep = null;
        
        return graphForEachTimeStep;
    }
}