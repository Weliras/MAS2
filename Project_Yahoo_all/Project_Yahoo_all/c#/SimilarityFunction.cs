namespace Project_Yahoo;

// Define a delegate type that matches the signature of the function you want to pass
public interface ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph);
}

public class CommonNeighbors : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();
        
        double similarity = intersection.Count;
        return similarity;
    }
}

public class JaccardCoefficient : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
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

public class AdamicAdarIndex : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = 0.0;
        foreach (var node in intersection)
        {
            var degree = graph.GetDegree(node);
            similarity += 1 / Math.Log(degree);
        }

        return similarity;
    }
}

public class PreferentialAttachment : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
    {
        var degree1 = graph.GetDegree(node1);
        var degree2 = graph.GetDegree(node2);
        
        double similarity = degree1 * degree2;

        return similarity;
    }
}

public class ResourceAllocationIndex : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = 0.0;
        foreach (var node in intersection)
        {
            var degree = graph.GetDegree(node);
            similarity += 1 / (double)(degree);
        }

        return similarity;
    }
}

public class CosineSimilarity : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();

        var degree1 = graph.GetDegree(node1);
        var degree2 = graph.GetDegree(node2);
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = intersection.Count / (Math.Sqrt(degree1 * degree2));

        if(similarity > 0)
            Console.WriteLine(similarity);
        return similarity;
    }
}

public class SorensenIndex : ISimilarityFunction
{
    public double CalculateSimilarity(int node1, int node2, Graph graph)
    {
        var neighbors1 = graph.GetNeighbors(node1).Keys.ToList();
        var neighbors2 = graph.GetNeighbors(node2).Keys.ToList();

        var degree1 = graph.GetDegree(node1);
        var degree2 = graph.GetDegree(node2);
        
        // Get the intersection of the two lists using LINQ
        var intersection = neighbors1.Intersect(neighbors2).ToList();

        double similarity = (2 * intersection.Count) / (double)(degree1 + degree2);

        if(similarity > 0)
            Console.WriteLine(similarity);
        return similarity;
    }
}