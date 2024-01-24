using System.Diagnostics;
using Project_Yahoo;


string pathToDirector = Path.Combine("..", "..", "Data");

Console.WriteLine("Loading as Temporal network per slices.");
MultiLayerGraph temporalNetwork = new MultiLayerGraph(pathToDirector);



Console.WriteLine($"Actors: {temporalNetwork.GetNumberOfActors()}");
int sumOfNodes = 0;
int sumOfEdges = 0;
foreach (var layerId in temporalNetwork.GetLayers())
{
    var g = temporalNetwork.GetGraphForLayer(layerId);
    Console.WriteLine($"Time Period: {layerId}, Nodes: {g.GetNumberOfNodes()}, Edges: {g.GetNumberOfEdges()}");
    sumOfNodes += g.GetNumberOfNodes();
    sumOfEdges += g.GetNumberOfEdges();
}
Console.WriteLine($"Total nodes: {sumOfNodes}");
Console.WriteLine($"Total edges: {sumOfEdges}");

// LINK PREDICTION

int year1 = 1;
int year2 = 2;
var graph1 = temporalNetwork.GetGraphForLayer(year1);
var graph2 = temporalNetwork.GetGraphForLayer(year2);
double threshold = 2;
int numberOfSamples = 5000;

// Use the Fisher-Yates shuffle algorithm to shuffle the list
Random random = new Random();
//List<int> shuffledList = graph1.GetNodes().OrderBy(x => random.Next()).ToList();
// Take the first N elements from the shuffled list
List<int> randomNodes = graph1.GetNodes().Take(numberOfSamples).ToList();

var predictedGraph = DataCalculations.PredictLinks(graph1, threshold, new CommonNeighbors(), randomNodes);
Console.WriteLine($"True [{year1}]: {graph1.GetNumberOfNodes()} nodes, {graph1.GetNumberOfEdges()} edges");
Console.WriteLine($"Predicted [{year2}] from [{year1}]: {predictedGraph.GetNumberOfNodes()} nodes, {predictedGraph.GetNumberOfEdges()} edges");
Console.WriteLine($"True [{year2}]: {graph2.GetNumberOfNodes()} nodes, {graph2.GetNumberOfEdges()} edges");

var confusionMatrix = predictedGraph.Compare(graph2, randomNodes);
foreach (var kvp in confusionMatrix)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

double sensitivity = ((double)confusionMatrix["TP"]) / (confusionMatrix["TP"] + confusionMatrix["FN"]);
double recall = ((double)confusionMatrix["TP"]) / (confusionMatrix["TP"] + confusionMatrix["FN"]);
double specificity = ((double)confusionMatrix["TN"]) / (confusionMatrix["FP"] + confusionMatrix["TN"]);
double precision = ((double)confusionMatrix["TP"]) / (confusionMatrix["TP"] + confusionMatrix["FP"]);
double fallout = ((double)confusionMatrix["FP"]) / (confusionMatrix["FP"] + confusionMatrix["TN"]);
double accuracy = ((double)(confusionMatrix["TP"] + confusionMatrix["TN"])) / (confusionMatrix["TP"] +
                                                                               confusionMatrix["TN"] + confusionMatrix["FP"] + confusionMatrix["FN"]);
                
Console.WriteLine($"Metrics:");
Console.WriteLine($"\tPredicted Edges: {predictedGraph.GetNumberOfEdges() - graph1.GetNumberOfEdges()}");
Console.WriteLine($"\tSensitivity: {Math.Round(sensitivity, 4)}");
Console.WriteLine($"\tRecall: {Math.Round(recall, 4)}");
Console.WriteLine($"\tSpecificity: {Math.Round(specificity, 4)}");
Console.WriteLine($"\tPrecision: {Math.Round(precision, 4)}");
Console.WriteLine($"\tFallout: {Math.Round(fallout, 4)}");
Console.WriteLine($"\tAccuracy: {Math.Round(accuracy, 4)}");




// Log nodes and edges for each layer
temporalNetwork.LogNodesAndEdgesForEachLayer(Path.Combine(pathToDirector, "Outputs", "nodesAndEdges.csv"));

// Timer for time measurement.
Stopwatch sw = new Stopwatch();

// Choosing candidates for influeance
var graph = temporalNetwork.GetGraphForLayer(1);
// Run simulation of influeance
List<int> valuesOfK = new List<int>() { 2, 4, 10 };
// k
using (StreamWriter writer = new StreamWriter(Path.Combine(pathToDirector, "Outputs", "influence.csv")))
{
    // Write header
    writer.WriteLine($"Ratio;MaxCandidates");
    foreach (var k in valuesOfK)
    {
        // Get candidates
        var candidates = graph.GetCandidates(maxCandidates:k);
        foreach (int candidate in candidates)
        {
            Console.WriteLine($"Candidate: {candidate}");
        }
        // iterations
        for (int i = 0; i < 500; i++)
        {
            // Run simulation
            var ratio = graph.ICSimulation(candidates, p:0.25f);
            // Log results
            writer.WriteLine($"{ratio};{k}");
        }
    }
}


Console.WriteLine($"Calculating density.");
sw.Restart();
temporalNetwork.LogDensity(Path.Combine(pathToDirector, "Outputs", "density.csv"));
sw.Stop();
Console.WriteLine($"Elapsed time in LogDensity: {sw.Elapsed}");

Console.WriteLine($"Calculating average clustering coefficient.");
sw.Restart();
temporalNetwork.LogAverageClusteringCoefficient(Path.Combine(pathToDirector, "Outputs", "averageClusteringCoefficient.csv"));
sw.Stop();
Console.WriteLine($"Elapsed time in LogAverageClusteringCoefficient: {sw.Elapsed}");

Console.WriteLine($"Calculating average degree.");
sw.Restart();
temporalNetwork.LogAverageDegreeForEachLayer(Path.Combine(pathToDirector, "Outputs", "averageOutDegrees.csv"));
sw.Stop();
Console.WriteLine($"Elapsed time in LogAverageOutDegreeForEachLayer: {sw.Elapsed}");

Console.WriteLine($"Calculating maximal degree.");
sw.Restart();
temporalNetwork.LogNodeWithMaxDegree(Path.Combine(pathToDirector, "Outputs", "maxOutDegrees.csv"));
sw.Stop();
Console.WriteLine($"Elapsed time in LogNodeWithMaxDegree: {sw.Elapsed}");

