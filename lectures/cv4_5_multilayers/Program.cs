namespace MultilayeredGraphs;

class Program
{
    static void Main()
    {
        string filePathEdges = "EUAir_Multiplex_Transport/Dataset/EUAirTransportation_multiplex.edges";
    
        var originalGraph = new LayeredGraph(filePathEdges);
        
        // Specified actor
        int actor = 1;
        // Specify set of layers
        List<int> layerIds = originalGraph.GetIdOfLayers().Take(5).ToList();
        // For actor
        Console.WriteLine($"Analysis for actor {actor}:");
        Console.WriteLine($"\tDegree centrality: {originalGraph.GetDegreeCentrality(actor, layerIds)}");
        Console.WriteLine($"\tDegree deviation: {originalGraph.GetDegreeDeviation(actor, layerIds)}");
        // neighbors
        Console.Write($"\tNeighbors: ");
        foreach (var neighbor in originalGraph.GetNeighbors(actor, layerIds))
        {
            Console.Write($"{neighbor} ");
        }
        Console.WriteLine();
        Console.WriteLine($"\tNeighborhood centrality: {originalGraph.GetNeighborhoodCentrality(actor, layerIds)}");
        Console.WriteLine($"\tConnective redundancy: {originalGraph.GetConnectiveRedundancy(actor, layerIds)}");
        Console.WriteLine($"\tExclusive Neighborhood centrality: {originalGraph.GetExclusiveNeighborhoodCentrality(actor, layerIds)}");
        // Relevances
        Console.WriteLine($"\tRelevance: {originalGraph.GetRelevance(actor, layerIds)}");
        Console.WriteLine($"\tExclusive Relevance: {originalGraph.GetExclusiveRelevance(actor, layerIds)}");
        // Occupation Centrality
        Console.WriteLine($"\tOccupation centrality: {originalGraph.GetOccupationCentralityUsingUniformRandomWalk(actor, layerIds, 10, 100_000)}");
        // 0.03433
        
        // Global
        Console.WriteLine($"Analysis for global values (all actors, all layers):");
        Console.WriteLine($"\tGlobal average degree centrality: {originalGraph.GetDegreeCentrality(originalGraph.GetIdOfLayers())}");
        Console.WriteLine($"\tGlobal average degree deviation: {originalGraph.GetDegreeDeviation(originalGraph.GetIdOfLayers())}");
        Console.WriteLine($"\tGlobal average neighborhood centrality: {originalGraph.GetNeighborhoodCentrality(originalGraph.GetIdOfLayers())}");
        Console.WriteLine($"\tGlobal average connective redundancy: {originalGraph.GetConnectiveRedundancy(originalGraph.GetIdOfLayers())}");
        Console.WriteLine($"\tGlobal average exclusive neighborhood centrality: {originalGraph.GetExclusiveNeighborhoodCentrality(originalGraph.GetIdOfLayers())}");
        Console.WriteLine($"\tGlobal average occupation centrality: {originalGraph.GetOccupationCentralityUsingUniformRandomWalk(originalGraph.GetIdOfLayers(), 10, 1000)}");

        
        // Relevence of all actors for all
        originalGraph.ExportTable(originalGraph.GetRelevance(), "relevance.csv");
        originalGraph.ExportTable(originalGraph.GetExclusiveRelevance(), "exclusive_relevance.csv");
        
        // Get flattened graph
        Graph flattenedUnweightedGraph = originalGraph.GetUnweightedFlattenedGraph(originalGraph.GetIdOfLayers());

    }
}
