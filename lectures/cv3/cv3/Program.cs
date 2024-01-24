namespace cv2;

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
        
        var graphForEachYear =
            DataLoader.LoadDBLPGraph(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow, maxYear);

        int type = 1;
        switch (type)
        {
            case 0:
                // For each frame write avg degree
                // Save degree, count to file and visualise it using python
                using (StreamWriter writer = new StreamWriter("outputs/outputs.csv"))
                {
                    writer.WriteLine($"Year;CountOfNodes;CountOfEdges;AverageDegree;AverageWeightedDegree;AverageClusteringCoefficient;IdOfSimplexWithMaxAvgWDegree;WDegreeOfSimplexWithMaxAvgWDegree");
                    foreach (var kvp in graphForEachYear)
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
                // Link predictions
                int year1 = 1956;
                int year2 = 1957;
                var graph1 = graphForEachYear[year1];
                var graph2 = graphForEachYear[year2];
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
                
        }
        
        
    }
}
