using System.Collections.Concurrent;

namespace cv2;

// Define a delegate type that matches the signature of the function you want to pass

public interface ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph);
}

public class CommonNeighbors<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();
        
        double similarity = intersection.Count;
        return similarity;
    }
}

public class JaccardCoefficient<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();
        var union = neighbors1.Union(neighbors2).ToList();
        
        double similarity = (double)intersection.Count / union.Count;
        return similarity;
    }
}

public class AdamicAdarIndex<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = 0.0;
        foreach (var node in intersection)
        {
            var degree = DataCalculations.CalculateLocalDegree(node, graph);
            similarity += 1 / Math.Log(degree);
        }

        return similarity;
    }
}

public class PreferentialAttachment<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var degree1 = DataCalculations.CalculateLocalDegree(node1, graph);
        var degree2 = DataCalculations.CalculateLocalDegree(node2, graph);
        
        double similarity = degree1 * degree2;

        return similarity;
    }
}

public class ResourceAllocationIndex<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = 0.0;
        foreach (var node in intersection)
        {
            var degree = DataCalculations.CalculateLocalDegree(node, graph);
            similarity += 1 / (double)(degree);
        }

        return similarity;
    }
}

public class CosineSimilarity<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        var degree1 = DataCalculations.CalculateLocalDegree(node1, graph);
        var degree2 = DataCalculations.CalculateLocalDegree(node2, graph);
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = intersection.Count / (Math.Sqrt(degree1 * degree2));

        if(similarity > 0)
            Console.WriteLine(similarity);
        return similarity;
    }
}

public class SorensenIndex<T> : ISimilarityFunction<T> where T : struct, IComparable<T>
{
    public double CalculateSimilarity(T node1, T node2, Graph<T> graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        var degree1 = DataCalculations.CalculateLocalDegree(node1, graph);
        var degree2 = DataCalculations.CalculateLocalDegree(node2, graph);
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = (2 * intersection.Count) / (double)(degree1 + degree2);

        if(similarity > 0)
            Console.WriteLine(similarity);
        return similarity;
    }
}