namespace HammingTests;

public class Tests
{
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
}
