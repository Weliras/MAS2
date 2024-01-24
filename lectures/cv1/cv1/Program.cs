using System.Collections.Concurrent;

namespace cv1;

using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main()
    {
        bool parallel = false;
        Stopwatch stopwatch = new Stopwatch();
        string[] filePaths = new[] {  "data/com-youtube.ungraph.txt","data/socfb-Penn94.mtx", "data/9606.protein.links.v10.5.txt" };

        foreach (var filePath in filePaths)
        {
            stopwatch.Start();
            var network = DataLoader.LoadNetwork(filePath);
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Console.WriteLine($"Number of loaded nodes: {network.NodesCount}");
            Console.WriteLine($"Number of loaded edges: {network.EdgeCount}");
            Console.WriteLine($"Function (LoadNetwork) execution time: {elapsedMilliseconds / 1000.0} seconds");
            
            // For each node calculate its degree
            stopwatch.Start();
            network.CalculateLocalDegrees(parallel: parallel);
            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Console.WriteLine($"Function (CalculateLocalDegree) execution time: {elapsedMilliseconds / 1000.0} seconds");
            // Get average degree
            Console.WriteLine($"Average degree is: {System.Math.Round(DataCalculations.CalculateAverageDegree(network.LocalDegrees), 2)}");
            // Get Maximum degree
            Console.WriteLine($"Max degree is: {DataCalculations.CalculateMaxDegree(network.LocalDegrees)}");
            
            // Generate histogram: For each degree count of nodes
            stopwatch.Start();
            DataCalculations.GenerateDegreeDistribution(network);
            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Console.WriteLine($"Function (GenerateDegreeDistribution) execution time: {elapsedMilliseconds / 1000.0} seconds");

            // For each node calculate its clustering coeff
            stopwatch.Start();
            network.CalculateLocalClusteringCoefficients(parallel:parallel);
            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Console.WriteLine($"Function (CalculateLocalClusteringCoefficient) execution time: {elapsedMilliseconds / 1000.0} seconds");
            
            // Generate histogram: For each degree avg clustering coeff
            stopwatch.Start();
            DataCalculations.GenerateClusteringEffect(network, parallel:parallel);
            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Console.WriteLine($"Function (GenerateClusteringEffect) execution time: {elapsedMilliseconds / 1000.0} seconds");
            
            stopwatch.Start();
            network.CalculateLocalCommonNeighbors(parallel:parallel);
            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            Console.WriteLine($"Function (CalculateLocalCommonNeighbors) execution time: {elapsedMilliseconds / 1000.0} seconds");
            // Get average degree
            Console.WriteLine($"Average number of common neighbors is: {System.Math.Round(DataCalculations.CalculateAverageNumberOfCommonNeighbors(network.LocalCommonNeighbors), 2)}");
            // Get Maximum degree
            Console.WriteLine($"Max number of common neighbors is: {DataCalculations.CalculateMaxNumberOfCommonNeighbors(network.LocalCommonNeighbors)}");
            
            // Dodělat paralelizaci a sousedy a měření času
            break;
        }
        
        
    }
}
