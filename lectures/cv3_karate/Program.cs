namespace cv3_2;

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Concurrent;

class Program
{
    static void KFoldCrossValidation(Graph graphTest, Graph graphOriginal, int k = 10, double threshold = 0.5)
    {
        int partitionSize = graphTest.NodesCount / k;
        int remainder = graphTest.NodesCount % k;
        
        List<List<int>> nodePartitions = new List<List<int>>();
        int index = 0;

        for (int i = 0; i < k; i++)
        {
            int size = partitionSize + (i < remainder ? 1 : 0);
            nodePartitions.Add(graphTest.Nodes.GetRange(index, size));
            index += size;
        }
        
        double sensitivity = 0, recall = 0, specificity = 0, precision = 0, fallout = 0, accuracy = 0;
        // Now make iteration of predictions and every time skip one part of nodes
        for (int i = 0; i < k; i++)
        {
            Console.WriteLine($"{i+1}. iteration of {k} splits.");
            var actualNodes = new List<List<int>>(nodePartitions);
            actualNodes.RemoveAt(i);
            var actualNodesConcatenated = actualNodes.SelectMany(x => x).ToList();

            Graph graphTestISplit = new Graph(graphTest, actualNodesConcatenated);
            Graph graphPredictedISplit = graphTestISplit.PredictLinks(threshold);
            Graph graphOriginalISplit = new Graph(graphOriginal, actualNodesConcatenated);
            
            Console.WriteLine($"\tPredicted: {graphPredictedISplit.NodesCount} nodes, {graphPredictedISplit.EdgeCount} edges");
            Console.WriteLine($"\tTrue: {graphOriginalISplit.NodesCount} nodes, {graphOriginalISplit.EdgeCount} edges");
            
            var confusionMatrix =  graphPredictedISplit.Compare(graphOriginalISplit);
            Console.WriteLine($"\tConfusion Matrix:");
            foreach (var kvp in confusionMatrix)
            {
                Console.WriteLine($"\t\t{kvp.Key}: {kvp.Value}");
            }

            sensitivity += ((double)confusionMatrix["TP"]) / (confusionMatrix["TP"] + confusionMatrix["FN"]);
            recall += ((double)confusionMatrix["TP"]) / (confusionMatrix["TP"] + confusionMatrix["FN"]);
            specificity += ((double)confusionMatrix["TN"]) / (confusionMatrix["FP"] + confusionMatrix["TN"]);
            precision += ((double)confusionMatrix["TP"]) / (confusionMatrix["TP"] + confusionMatrix["FP"]);
            fallout += ((double)confusionMatrix["FP"]) / (confusionMatrix["FP"] + confusionMatrix["TN"]);
            accuracy += ((double)(confusionMatrix["TP"] + confusionMatrix["TN"])) / (confusionMatrix["TP"] +
                confusionMatrix["TN"] + confusionMatrix["FP"] + confusionMatrix["FN"]);
                
            
        }
        Console.WriteLine($"Metrics:");
        Console.WriteLine($"\tSensitivity: {Math.Round(sensitivity / k, 4)}");
        Console.WriteLine($"\tRecall: {Math.Round(recall / k, 4)}");
        Console.WriteLine($"\tSpecificity: {Math.Round(specificity / k, 4)}");
        Console.WriteLine($"\tPrecision: {Math.Round(precision / k, 4)}");
        Console.WriteLine($"\tFallout: {Math.Round(fallout / k, 4)}");
        Console.WriteLine($"\tAccuracy: {Math.Round(accuracy / k, 4)}");

        
    }
    static void Main()
    {
        string filePath = "soc-karate.mtx";
        double threshold = 0.6;
    
        var originalGraph = new Graph(filePath);
        originalGraph.ChangeSimilarityFunctions(new CosineSimilarity());
        //var graphForEachYear = DataLoader.LoadDBLPGraph(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow, maxYear);
        
        // Create second graph and in it remove some part of edges and run prediction on it.
        var graphTest = new Graph(originalGraph);
        graphTest.RemovePartOfEdges(0.2);
        
        KFoldCrossValidation(graphTest, originalGraph, 10, threshold);
        
    }
}
