using System.Collections.Concurrent;

namespace cv1;

public class DataLoader
{
    public static Graph LoadNetwork(string filePath)
    {
        Console.WriteLine($"Loading network: {filePath}");
        Graph network = new Graph();
        //ConcurrentDictionary<int, ConcurrentDictionary<int, bool>> dictionaryOfKeys = new ConcurrentDictionary<int, ConcurrentDictionary<int, bool>>();
        try
        {
            // Open the file for reading using StreamReader
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read and display the contents line by line
                while (reader.ReadLine() is { } line)
                {
                    if (line.StartsWith("#"))
                        continue;
                    var parts = line.Replace("\n", "").Split();
                    int idFrom = int.Parse(parts[0]);
                    int idTo = int.Parse(parts[1]);
                    //Console.WriteLine($"{idFrom}->{idTo}");
                    
                    // Add edge
                    network.AddEdge(idFrom, idTo);
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("The file does not exist.");
        }
        catch (IOException e)
        {
            Console.WriteLine($"An error occurred while reading the file: {e.Message}");
        }

        return network;
    }
}