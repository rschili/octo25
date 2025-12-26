namespace ParallelPathFinderTests;

public class DiagnosticTests
{
    [Test]
    public async Task VerifyWaitingLogic_StepByStep()
    {
        // Trace through what SHOULD happen with your real data
        const string partialGraph = """
        graph G {
        "AA" -- "BA" [timestep="even"];
        "BA" -- "CA" [timestep="odd"];
        "CA" -- "DA" [timestep="odd"];
        }
        """;
        
        Graph graph = DotFileReader.LoadFromDot(partialGraph);
        var result = await ParallelPathFinder.FindPathsAsync(
            graph.NodesByName["AA"], 
            graph.NodesByName["DA"]);
        
        Console.WriteLine($"Time: {result.ShortestTime}");
        var path = result.ShortestPaths[0];
        Console.WriteLine($"Nodes: {path.Count}");
        
        for (int i = 0; i < path.Count; i++)
        {
            var step = path[i];
            Console.WriteLine($"  [{i}] {step.Node.Name} - Waited: {step.Waited}");
        }
        
        // Starting at time 0 (even)
        // AA -> BA: edge is even, time is even → no wait, arrive at time 1 (odd)
        await Assert.That(path[0].Node.Name).IsEqualTo("AA");
        await Assert.That(path[0].Waited).IsFalse();
        
        await Assert.That(path[1].Node.Name).IsEqualTo("BA");
        await Assert.That(path[1].Waited).IsFalse(); // no wait needed
        
        // BA -> CA: edge is odd, time is 1 (odd) → no wait, arrive at time 2 (even)
        await Assert.That(path[2].Node.Name).IsEqualTo("CA");
        await Assert.That(path[2].Waited).IsFalse(); // no wait needed
        
        // CA -> DA: edge is odd, time is 2 (even) → WAIT!, arrive at time 4 (even)
        await Assert.That(path[3].Node.Name).IsEqualTo("DA");
        await Assert.That(path[3].Waited).IsTrue(); // SHOULD wait!
        
        // Total: 4 steps for 4 nodes = 1 wait occurred
        await Assert.That(result.ShortestTime).IsEqualTo(4u);
    }
    
    [Test]
    public async Task VerifyRealDataFirstFewSteps()
    {
        // Use actual data from your input file
        var graph = DotFileReader.LoadFromDot(DataSet.Value);
        var result = await ParallelPathFinder.FindPathsAsync(
            graph.NodesByName["AA"],
            graph.NodesByName["ZZ"]);
        
        Console.WriteLine($"\nReal data analysis:");
        Console.WriteLine($"Total time: {result.ShortestTime}");
        Console.WriteLine($"Total nodes in path: {result.ShortestPaths[0].Count}");
        Console.WriteLine($"Expected steps if no waits: {result.ShortestPaths[0].Count - 1}");
        Console.WriteLine($"Actual steps: {result.ShortestTime}");
        Console.WriteLine($"Number of waits: {result.ShortestTime - (result.ShortestPaths[0].Count - 1)}");
        
        var path = result.ShortestPaths[0];
        Console.WriteLine("\nFirst 10 steps:");
        for (int i = 0; i < Math.Min(10, path.Count); i++)
        {
            Console.WriteLine($"  [{i}] {path[i].Node.Name} - Waited: {path[i].Waited}");
        }
        
        int waitCount = path.Count(step => step.Waited);
        Console.WriteLine($"\nTotal waits in path: {waitCount}");
        
        // This should match: pathLength - 1 + waitCount = total time
        await Assert.That((uint)(path.Count - 1 + waitCount)).IsEqualTo(result.ShortestTime);
    }
}
