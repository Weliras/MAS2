namespace Project_Yahoo;

using System.Collections.Concurrent;
using System.Diagnostics;

public class DataCalculations
{
    public static Graph PredictLinks(Graph graph, double threshold, ISimilarityFunction similarityFunction, List<int> chosenNodes)
    {
        void PredictNewEdges(Graph predictedGraph)
        {
            Parallel.ForEach(chosenNodes, node1 =>
            {
                foreach (var node2 in chosenNodes)
                {
                    var similarityValue = similarityFunction.CalculateSimilarity(node1, node2, graph);
                    if (similarityValue > threshold)
                    {
                        predictedGraph.AddUndirectedEdge(node1, node2);
                    }
                    
                }
                /*foreach (var node2 in from node2 in predictedGraph.GetNodes().AsParallel() let similarityValue = similarityFunction.CalculateSimilarity(node1, node2, graph) where similarityValue > threshold select node2)
                {
                    //AddNode(node1);
                    //AddNode(node2);
                    predictedGraph.AddUndirectedEdge(node1, node2);
                }*/
            });
        }
        
        var predictedGraph = new Graph(graph);
        
        Console.WriteLine("PredictNewEdges");
        var sw = new Stopwatch();
        sw.Restart();
        PredictNewEdges(predictedGraph);
        sw.Stop();
        Console.WriteLine($"Elapsed time in PredictNewEdges: {sw.Elapsed}");
        
        return predictedGraph;
    }
}