using System.Text.RegularExpressions;

public enum Timestep
{
    Even,
    Odd
}

public class Node
{
    public string Name { get; } // "AA", "AB", etc.
    public List<Connection> Edges { get; } = new();
    public uint DiscardThreshold; // Tracks the FASTEST time we've ever seen a node reached. We use this to discard paths that are strictly slower.

    public Node(string name)
    {
        Name = name;
        DiscardThreshold = uint.MaxValue;
    }
}

public class Connection // Represents a pointed connection to another node. A undirected edge will create two Connections.
{
    public Node Target { get; }
    public Timestep Timestep { get; }

    public Connection(Node target, Timestep timestep)
    {
        Target = target;
        Timestep = timestep;
    }
}

// This graph implementation is stateful, it can only run once!
public class Graph
{
    public List<Node> Nodes { get; }
    public int EdgeCount { get; } // just for testing purposes to make sure we read the whole file
    
    public Dictionary<string, Node> NodesByName  { get; }

    public Graph(List<Node> nodes, Dictionary<string, Node> nodesByName, int edgeCount)
    {
        Nodes = nodes;
        NodesByName = nodesByName;
        EdgeCount = edgeCount;
    }
}

public static class DotFileReader
{
    public static Graph LoadFromDot(string dotContent)
    {
        var nodes = new List<Node>();
        var nodesByName = new Dictionary<string, Node>();
        var lines = dotContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Regex to parse: "NodeA" -- "NodeB" [timestep="type"];
        var edgePattern = new Regex(@"\s*""(.+?)""\s*--\s*""(.+?)""\s*\[timestep=""(.+?)""\];");
        // This is totally not robust DOT parsing, but sufficient for our limited use case.
        int edgeCount = 0;
        
        foreach (var line in lines)
        {
            var match = edgePattern.Match(line);
            if (!match.Success) continue; // Skip lines like "graph G {" or "}"

            string sourceName = match.Groups[1].Value;
            string targetName = match.Groups[2].Value;
            string timestepStr = match.Groups[3].Value.ToLower();

            // Fail if a node connects to itself
            if (sourceName == targetName)
            {
                throw new ArgumentException($"Self-loop detected: Node '{sourceName}' cannot connect to itself.");
            }

            Timestep timestep = timestepStr switch
            {
                "odd" => Timestep.Odd,
                "even" => Timestep.Even,
                _ => throw new ArgumentException($"Invalid parity value '{timestepStr}'. Only 'odd' or 'even' are allowed.")
            };

            // Get or create source node
            if (!nodesByName.TryGetValue(sourceName, out var sourceNode))
            {
                sourceNode = new Node(sourceName);
                nodesByName[sourceName] = sourceNode;
                nodes.Add(sourceNode);
            }

            // Get or create target node
            if (!nodesByName.TryGetValue(targetName, out var targetNode))
            {
                targetNode = new Node(targetName);
                nodesByName[targetName] = targetNode;
                nodes.Add(targetNode);
            }

            // Check for duplicate connections before adding
            bool sourceHasDuplicate = sourceNode.Edges.Any(e => e.Target == targetNode && e.Timestep == timestep);
            bool targetHasDuplicate = targetNode.Edges.Any(e => e.Target == sourceNode && e.Timestep == timestep);

            if (sourceHasDuplicate || targetHasDuplicate)
            {
                Console.WriteLine($"Warning: Duplicate connection detected between '{sourceName}' and '{targetName}' with timestep '{timestepStr}'. Skipping.");
                continue;
            }

            // Add Undirected Edges (A -> B and B -> A)
            sourceNode.Edges.Add(new Connection(targetNode, timestep));
            targetNode.Edges.Add(new Connection(sourceNode, timestep));
            edgeCount++;
        }
    
        return new Graph(nodes, nodesByName, edgeCount);
    }
}