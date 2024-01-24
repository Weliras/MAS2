namespace DBLPAsMultilayer;

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Concurrent;



class Program
{
    static void Main()
    {
        string yearsFilepath = Path.Combine("coauth-DBLP", "coauth-DBLP-times.txt");
        string numberOfNodesInSimplexesFilepath = Path.Combine("coauth-DBLP", "coauth-DBLP-nverts.txt");
        string nodesInSimplexesFilepath = Path.Combine("coauth-DBLP", "coauth-DBLP-simplices.txt");
        int lengthOfTimeWindow = 1;
        int maxYear = 1980;
        
        int type = 2;
        switch (type)
        {
            case 0:
                var graphForEachYear0 = DataLoader.LoadDBLPGraph(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow, maxYear);
                // For each frame write avg degree
                // Save degree, count to file and visualise it using python
                using (StreamWriter writer = new StreamWriter("outputs/outputs.csv"))
                {
                    writer.WriteLine($"Year;CountOfNodes;CountOfEdges;AverageDegree;AverageWeightedDegree;AverageClusteringCoefficient;IdOfSimplexWithMaxAvgWDegree;WDegreeOfSimplexWithMaxAvgWDegree");
                    foreach (var kvp in graphForEachYear0)
                    {
                        Console.WriteLine($"Year: {kvp.Key}");
                
                        kvp.Value.CalculateLocalDegrees(true);
                        var avgDegree = DataCalculations.CalculateAverageDegree(kvp.Value.LocalDegrees);
                        Console.WriteLine($"\tAverage Degree: {avgDegree}");
                
                        kvp.Value.CalculateLocalWeightedDegrees(true);
                        var avgWeightedDegree = DataCalculations.CalculateAverageDegree(kvp.Value.LocalWeightedDegrees);
                        Console.WriteLine($"\tAverage weighted Degree: {avgWeightedDegree}");

                        kvp.Value.CalculateLocalClusteringCoefficients(true);
                        var avgClusteringCoef = kvp.Value.LocalClusteringCoefficient.Values.Sum() /
                                                kvp.Value.LocalClusteringCoefficient.Keys.Count;
                        Console.WriteLine($"\tAverage Clustering Coefficient: {avgClusteringCoef}");
                
                        var res = kvp.Value.CalculateSimplexWithMaxAvgWeight(parallel:true);
                        var simplexId = res.Item1;
                        var maxWDegreeOfSimplex = res.Item2;
                        Console.WriteLine($"\tSimplex with maximal average weight: {simplexId}, {maxWDegreeOfSimplex}");

                
                        writer.WriteLine($"{kvp.Key};{kvp.Value.NodesCount};{kvp.Value.EdgeCount};{avgDegree};{avgWeightedDegree};{avgClusteringCoef};{simplexId};{maxWDegreeOfSimplex}");
                    }
                }

                break;
            
            case 1:
                var graphForEachYear1 = DataLoader.LoadDBLPGraph(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow, maxYear);

                // Link predictions
                int year1 = 1956;
                int year2 = 1957;
                var graph1 = graphForEachYear1[year1];
                var graph2 = graphForEachYear1[year2];
                double threshold = 0.5;
                
                graph1.ChangeSimilarityFunctions(new SorensenIndex<int>());
                var predictedGraph = DataCalculations.PredictLinks(graph1, threshold);
                
                
                Console.WriteLine($"True [{year1}]: {graph1.NodesCount} nodes, {graph1.EdgeCount} edges");
                Console.WriteLine($"Predicted [{year2}] from [{year1}]: {predictedGraph.NodesCount} nodes, {predictedGraph.EdgeCount} edges");
                Console.WriteLine($"True [{year2}]: {graph2.NodesCount} nodes, {graph2.EdgeCount} edges");

                var confusionMatrix = predictedGraph.Compare(graph2);
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
                Console.WriteLine($"\tSensitivity: {Math.Round(sensitivity, 4)}");
                Console.WriteLine($"\tRecall: {Math.Round(recall, 4)}");
                Console.WriteLine($"\tSpecificity: {Math.Round(specificity, 4)}");
                Console.WriteLine($"\tPrecision: {Math.Round(precision, 4)}");
                Console.WriteLine($"\tFallout: {Math.Round(fallout, 4)}");
                Console.WriteLine($"\tAccuracy: {Math.Round(accuracy, 4)}");
                
                break;
            case 2:
                var graphForEachYear2 = new LayeredGraph(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow, maxYear);
                // Make analysis
                // Specified actor
                int actor = 3;
                // Specify set of layers
                List<int> layerIds = graphForEachYear2.GetIdOfLayers().Take(50).ToList();
                // For actor
                Console.WriteLine($"Analysis for actor {actor}:");
                Console.WriteLine($"\tDegree centrality: {graphForEachYear2.GetDegreeCentrality(actor, layerIds)}");
                Console.WriteLine($"\tDegree deviation: {graphForEachYear2.GetDegreeDeviation(actor, layerIds)}");
                // neighbors
                Console.Write($"\tNeighbors: ");
                foreach (var neighbor in graphForEachYear2.GetNeighbors(actor, layerIds))
                {
                    Console.Write($"{neighbor} ");
                }
                Console.WriteLine();
                Console.WriteLine($"\tNeighborhood centrality: {graphForEachYear2.GetNeighborhoodCentrality(actor, layerIds)}");
                Console.WriteLine($"\tConnective redundancy: {graphForEachYear2.GetConnectiveRedundancy(actor, layerIds)}");
                Console.WriteLine($"\tExclusive Neighborhood centrality: {graphForEachYear2.GetExclusiveNeighborhoodCentrality(actor, layerIds)}");


        
                // Global
                Console.WriteLine($"Analysis for global values (all actors, set of layers):");
                Console.WriteLine($"\tGlobal average degree centrality: {graphForEachYear2.GetDegreeCentrality(layerIds)}");
                Console.WriteLine($"\tGlobal average degree deviation: {graphForEachYear2.GetDegreeDeviation(layerIds)}");

                
                break;
        }
        
        
    }
}
