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
        
        var graphForEachYear =
            DataLoader.LoadDBLPGraph(numberOfNodesInSimplexesFilepath, nodesInSimplexesFilepath, yearsFilepath, lengthOfTimeWindow);

        foreach (var kvp in graphForEachYear)
        {
            Console.WriteLine($"Year {kvp.Key}: {kvp.Value.NodesCount} nodes, {kvp.Value.EdgeCount} edges.");
        }
        // For each frame write avg degree
        // Save degree, count to file and visualise it using python
        using (StreamWriter writer = new StreamWriter("outputs/outputs.csv"))
        {
            writer.WriteLine($"Year;AverageDegree;AverageWeightedDegree;AverageClusteringCoefficient;IdOfSimplexWithMaxAvgWDegree;WDegreeOfSimplexWithMaxAvgWDegree");
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

                
                writer.WriteLine($"{kvp.Key};{avgDegree};{avgWeightedDegree};{avgClusteringCoef};{simplexId};{maxWDegreeOfSimplex}");
            }
        }
        
    }
}
