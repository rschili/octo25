using System.Diagnostics;

// Load graph from data
Console.WriteLine("Loading graph from data...");
var graph = DotFileReader.LoadFromDot(DataSet.Value);
Console.WriteLine($"Loaded {graph.Nodes.Count} nodes and {graph.EdgeCount} edges");

// Get start and target nodes
var startNode = graph.NodesByName["AA"];
var targetNode = graph.NodesByName["ZZ"];

Console.WriteLine($"Finding shortest paths from {startNode.Name} to {targetNode.Name}...");

// Run pathfinder
var stopwatch = Stopwatch.StartNew();
var result = await ParallelPathFinder.FindPathsAsync(startNode, targetNode);
stopwatch.Stop();

// Display results
Console.WriteLine();
Console.WriteLine($"Shortest time: {result.ShortestTime} steps");
Console.WriteLine($"Number of shortest paths: {result.PathCount}");
Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds}ms");

// Display first few paths for debugging
Console.WriteLine();
Console.WriteLine("Shortest paths:");
foreach (var path in result.ShortestPaths)
{
    var pathStr = string.Join(" -> ", path.Select(n => n.Name));
    Console.WriteLine($"  {pathStr}");
}

