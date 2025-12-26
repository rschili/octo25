namespace ParallelPathFinderTests;

public class Tests
{
    // Test data provided with the assignment. The correct answer is 1 shortest path (AA -- AB -- BB -- CB -- ZZ)
    private const string TestDataSet = """
    graph G {
    "AA" -- "BA" [timestep="odd"];
    "AA" -- "AB" [timestep="even"];
    "AB" -- "BB" [timestep="odd"];
    "AB" -- "AC" [timestep="even"];
    "AC" -- "BC" [timestep="odd"];
    "BA" -- "CA" [timestep="even"];
    "BA" -- "BB" [timestep="odd"];
    "BB" -- "CB" [timestep="even"];
    "BB" -- "BC" [timestep="odd"];
    "BC" -- "ZZ" [timestep="odd"];
    "CA" -- "CB" [timestep="even"];
    "CB" -- "ZZ" [timestep="odd"];
    }
    """;

    [Test]
    public async Task Graph_LoadFromDot_LoadsSuccessfully()
    {
        Graph graph = DotFileReader.LoadFromDot(TestDataSet);
        
        // Verify edge count
        await Assert.That(graph.EdgeCount).IsEqualTo(12);
        
        // Verify node count
        await Assert.That(graph.Nodes.Count).IsEqualTo(9);
        
        // Verify specific nodes exist
        var nodeAA = graph.NodesByName["AA"];
        await Assert.That(nodeAA).IsNotNull();
        await Assert.That(nodeAA!.Name).IsEqualTo("AA");
        await Assert.That(nodeAA.Edges.Count).IsEqualTo(2);
        
        // Check AA's edges
        var aaToBA = nodeAA.Edges.FirstOrDefault(e => e.Target.Name == "BA");
        await Assert.That(aaToBA).IsNotNull();
        await Assert.That(aaToBA!.Timestep).IsEqualTo(Timestep.Odd);
        
        var aaToAB = nodeAA.Edges.FirstOrDefault(e => e.Target.Name == "AB");
        await Assert.That(aaToAB).IsNotNull();
        await Assert.That(aaToAB!.Timestep).IsEqualTo(Timestep.Even);
        
        // Verify BB which has most edges (4)
        var nodeBB = graph.NodesByName["BB"];
        await Assert.That(nodeBB.Edges.Count).IsEqualTo(4);
        
        // Check some of BB's edges
        var bbToAB = nodeBB.Edges.FirstOrDefault(e => e.Target.Name == "AB");
        await Assert.That(bbToAB!.Timestep).IsEqualTo(Timestep.Odd);
        
        var bbToCB = nodeBB.Edges.FirstOrDefault(e => e.Target.Name == "CB");
        await Assert.That(bbToCB!.Timestep).IsEqualTo(Timestep.Even);
        
        // Verify end node ZZ
        var nodeZZ = graph.NodesByName["ZZ"];
        await Assert.That(nodeZZ.Edges.Count).IsEqualTo(2);
        
        var zzToBC = nodeZZ.Edges.FirstOrDefault(e => e.Target.Name == "BC");
        await Assert.That(zzToBC!.Timestep).IsEqualTo(Timestep.Odd);
        
        var zzToCB = nodeZZ.Edges.FirstOrDefault(e => e.Target.Name == "CB");
        await Assert.That(zzToCB!.Timestep).IsEqualTo(Timestep.Odd);

        // Verify NodesByName contains all nodes from Nodes list
        foreach (var node in graph.Nodes)
        {
            await Assert.That(graph.NodesByName.ContainsKey(node.Name)).IsTrue();
            await Assert.That(graph.NodesByName[node.Name]).IsEqualTo(node);
        }

        // Verify NodesByName count matches Nodes count
        await Assert.That(graph.NodesByName.Count).IsEqualTo(graph.Nodes.Count);
    }

    [Test]
    public async Task ParallelPathFinder_FindsShortestPath()
    {
        // Load the test graph
        Graph graph = DotFileReader.LoadFromDot(TestDataSet);
        
        // Get start and end nodes
        var startNode = graph.NodesByName["AA"];
        var endNode = graph.NodesByName["ZZ"];
        
        // Find shortest paths
        var result = await ParallelPathFinder.FindPathsAsync(startNode, endNode);
        
        // Verify we found exactly 1 shortest path as documented in the test data
        await Assert.That(result.PathCount).IsEqualTo(1);
        
        // Verify the shortest time (AA->AB is even/1 step, AB->BB is odd/1 step, BB->CB is even/1 step, CB->ZZ is odd/1 step = 4 steps)
        await Assert.That(result.ShortestTime).IsEqualTo(4u);
        
        // Verify the path itself: AA -- AB -- BB -- CB -- ZZ
        var path = result.ShortestPaths[0];
        await Assert.That(path.Count).IsEqualTo(5);
        await Assert.That(path[0].Name).IsEqualTo("AA");
        await Assert.That(path[1].Name).IsEqualTo("AB");
        await Assert.That(path[2].Name).IsEqualTo("BB");
        await Assert.That(path[3].Name).IsEqualTo("CB");
        await Assert.That(path[4].Name).IsEqualTo("ZZ");
    }
}
