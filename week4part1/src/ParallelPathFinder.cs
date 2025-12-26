using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Channels;

public record Tracer(Node CurrentPosition, uint CurrentSteps, TraceHistory History);

public record PathFinderResult(uint ShortestTime, ImmutableList<List<Node>> ShortestPaths)
{
    public int PathCount => ShortestPaths.Count;
}

// 1. Memory Optimization: A Class with Parent pointer (single linked list) instead of ImmutableList
public class TraceHistory
{
    public Node Node { get; }
    public TraceHistory? Parent { get; }

    public TraceHistory(Node node, TraceHistory? parent)
    {
        Node = node;
        Parent = parent;
    }

    // Helper to reconstruct path only when needed (at the end)
    public List<Node> ToList()
    {
        var list = new List<Node>();
        var current = this;
        while (current != null)
        {
            list.Add(current.Node);
            current = current.Parent;
        }
        list.Reverse();
        return list;
    }
}

public static class ParallelPathFinder
{
    public const int MaxSteps = 100;

    private class SharedState
    {
        public required Node DestinationNode { get; init; }
        public uint GlobalShortestTime = uint.MaxValue;
        // Starts with 1 for the initial item. When we reach zero, we close the channel.
        public int PendingWork = 1; 

        public readonly ConcurrentBag<(List<Node> Path, uint Time)> WinningPaths = [];
        public readonly ConcurrentDictionary<Node, uint> EarliestVisitorPerNode = new();
    }

    public static async Task<PathFinderResult> FindPathsAsync(Node startNode, Node destinationNode)
    {
        var state = new SharedState { DestinationNode = destinationNode };
        var channel = Channel.CreateUnbounded<Tracer>();
        await channel.Writer.WriteAsync(new Tracer(startNode, 0, new TraceHistory(startNode, null)));
        state.EarliestVisitorPerNode[startNode] = 0;

        // Spin off multiple workers to process incoming tracers in parallel
        await Parallel.ForEachAsync(channel.Reader.ReadAllAsync(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (tracer, ct) =>
        {
            // We don't await anything inside, we return ValueTask.CompletedTask implicitely.
            ProcessSingleTracer(tracer, channel.Writer, state);
            return ValueTask.CompletedTask;
        });

        var bestTime = state.GlobalShortestTime;
        // The WinningPaths bag may contain paths with longer times, filter them out.
        var bestPaths = state.WinningPaths
            .Where(result => result.Time == bestTime)
            .Select(result => result.Path)
            .ToImmutableList();

        return new PathFinderResult(bestTime, bestPaths);
    }

    private static void ProcessSingleTracer(Tracer tracer, ChannelWriter<Tracer> writer, SharedState state)
    {
        try
        {
            // --- DISCARD CONDITIONS ---
            if (tracer.CurrentSteps > MaxSteps) return;
            if (tracer.CurrentSteps > state.GlobalShortestTime) return;

            // Late node visit discard (another tracer already visited this node earlier, we cannot catch up)
            uint earliestVisitor = state.EarliestVisitorPerNode.GetOrAdd(tracer.CurrentPosition, tracer.CurrentSteps);
            if (tracer.CurrentSteps >= earliestVisitor + 2) // on +1 we may still catch up
                return;

            if (tracer.CurrentSteps < earliestVisitor)
                state.EarliestVisitorPerNode.TryUpdate(tracer.CurrentPosition, tracer.CurrentSteps, earliestVisitor);

            // --- TARGET CHECK ---
            if (tracer.CurrentPosition == state.DestinationNode)
            {
                ProcessWin(tracer, state);
                return;
            }

            // --- BRANCHING ---
            foreach (var edge in tracer.CurrentPosition.Edges)
            {
                // step 1 or 2, depending on timestep match (waiting 1 turn at the node if needed)
                uint stepIncrement = (tracer.CurrentSteps % 2 == 0) ^ (edge.Timestep == Timestep.Even) ? 2u : 1u;

                // Increment BEFORE writing to prevent race condition where another worker
                // could consume and complete the item before we account for it
                Interlocked.Increment(ref state.PendingWork);

                // TryWrite is strictly better here than WriteAsync because we want
                // the inner loop to be synchronous and fast. 
                // Unbounded channels never block on write, so TryWrite always returns true.
                writer.TryWrite(new Tracer(
                    edge.Target,
                    tracer.CurrentSteps + stepIncrement,
                    new TraceHistory(edge.Target, tracer.History)));
            }
        }
        finally
        {
            // Decrement for the item we just consumed
            // If this brings the count to 0, we're done - no more work exists
            if (Interlocked.Decrement(ref state.PendingWork) == 0)
            {
                // This closes the channel, causing ReadAllAsync() to stop yielding items,
                // which causes Parallel.ForEachAsync to complete gracefully.
                writer.Complete();
            }
        }
    }

    private static void ProcessWin(Tracer trace, SharedState state)
    {
        uint currentBest = state.GlobalShortestTime;
        if (trace.CurrentSteps < currentBest)
        {
            uint oldValue;
            do
            {
                oldValue = state.GlobalShortestTime;
                if (trace.CurrentSteps >= oldValue) break;
            }
            while (Interlocked.CompareExchange(ref state.GlobalShortestTime, trace.CurrentSteps, oldValue) != oldValue);
            
            state.WinningPaths.Add((trace.History.ToList(), trace.CurrentSteps));
        }
        else if (trace.CurrentSteps == currentBest)
        {
            state.WinningPaths.Add((trace.History.ToList(), trace.CurrentSteps));
        }
    }
}