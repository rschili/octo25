using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Channels;

public record Tracer(Node CurrentPosition, uint CurrentTime, ImmutableList<Node> History);

public record PathFinderResult(uint ShortestTime, ImmutableList<ImmutableList<Node>> ShortestPaths)
{
    public int PathCount => ShortestPaths.Count;
}

public static class ParallelPathFinder
{
    // Helper class to hold shared mutable state
    private class SharedState
    {
        public uint GlobalShortestTime = uint.MaxValue;
        public int ItemsInQueue = 1; // Seeded with 1 item
        public readonly ConcurrentBag<(ImmutableList<Node> Path, uint Time)> WinningPaths = new();
        public readonly object QueueLock = new();
    }

    public static async Task<PathFinderResult> FindPathsAsync(Node startNode, Node targetNode, Graph graph)
    {
        // Shared state for all workers
        var state = new SharedState();
        
        // 1. Setup Channels (The Queue)
        var channel = Channel.CreateUnbounded<Tracer>();

        // 2. Initial Seed
        Interlocked.Exchange(ref startNode.DiscardThreshold, 0);
        await channel.Writer.WriteAsync(
            new Tracer(startNode, 0, ImmutableList<Node>.Empty.Add(startNode))
        );

        // 3. Start Consumers (Workers)
        var reader = channel.Reader;
        List<Task> workers = new();

        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            workers.Add(Task.Run(() => ProcessTracers(reader, channel.Writer, targetNode, state)));
        }

        await Task.WhenAll(workers);
        channel.Writer.Complete();

        // Filter and return results
        var bestTime = state.GlobalShortestTime;
        var bestPaths = state.WinningPaths
            .Where(result => result.Time == bestTime)
            .Select(result => result.Path)
            .ToImmutableList();

        return new PathFinderResult(bestTime, bestPaths);
    }

    private static async Task ProcessTracers(
        ChannelReader<Tracer> reader,
        ChannelWriter<Tracer> writer,
        Node targetNode,
        SharedState state)
    {
        while (true)
        {
            Tracer? trace = null;
            
            // Try to read an item
            if (!reader.TryRead(out trace))
            {
                // Queue is empty, check if we should wait or exit
                lock (state.QueueLock)
                {
                    if (state.ItemsInQueue == 0)
                    {
                        // No more work, exit
                        return;
                    }
                }
                
                // Wait a bit for new items
                await Task.Delay(1);
                continue;
            }
            
            // Decrement because we took an item
            lock (state.QueueLock)
            {
                state.ItemsInQueue--;
            }

            // --- STOP CONDITIONS ---

            // A. Global Cutoff: If we already found a path at T=20, 
            // any active trace at T=21 is useless.
            if (trace.CurrentTime > state.GlobalShortestTime) continue;

            // B. Node Cutoff: If another trace reached this node at T=10, 
            // and we are here at T=12, we die.
            // BUT: If we are here at T=10, we continue (to preserve our unique history).
            uint recordTime = trace.CurrentPosition.DiscardThreshold;
            if (trace.CurrentTime > recordTime) continue;

            // Update the record if we are faster (thread-safe with Interlocked)
            uint currentThreshold;
            do
            {
                currentThreshold = trace.CurrentPosition.DiscardThreshold;
                if (trace.CurrentTime >= currentThreshold)
                    break; // Someone else already set a better or equal time
            }
            while (Interlocked.CompareExchange(ref trace.CurrentPosition.DiscardThreshold, trace.CurrentTime, currentThreshold) != currentThreshold);

            // --- TARGET CHECK ---
            if (trace.CurrentPosition == targetNode)
            {
                ProcessWin(trace, state);
                continue;
            }

            // --- BRANCHING ---
            int newItemsAdded = 0;
            foreach (var edge in trace.CurrentPosition.Edges)
            {
                // XOR: if timestep differ, we wait (add 2), otherwise proceed (add 1)
                uint timeIncrement = (trace.CurrentTime % 2 == 0) ^ (edge.Timestep == Timestep.Even) ? 2u : 1u;

                if (writer.TryWrite(new Tracer(
                    edge.Target,
                    trace.CurrentTime + timeIncrement,
                    trace.History.Add(edge.Target))))
                {
                    newItemsAdded++;
                }
            }
            
            // Update the queue counter
            if (newItemsAdded > 0)
            {
                lock (state.QueueLock)
                {
                    state.ItemsInQueue += newItemsAdded;
                }
            }
        }
    }

    private static void ProcessWin(Tracer trace, SharedState state)
    {
        // Thread-safe update of the global best score
        uint currentBest = state.GlobalShortestTime;
        if (trace.CurrentTime < currentBest)
        {
            // We found a NEW faster path - update the score (thread-safe)
            uint oldValue;
            do
            {
                oldValue = state.GlobalShortestTime;
                if (trace.CurrentTime >= oldValue)
                    break; // Someone else already found an equal or better path
            }
            while (Interlocked.CompareExchange(ref state.GlobalShortestTime, trace.CurrentTime, oldValue) != oldValue);
            
            // Add this path (we filter old paths at the end)
            state.WinningPaths.Add((trace.History, trace.CurrentTime));
        }
        else if (trace.CurrentTime == currentBest)
        {
            // We found another path with the SAME best score. Keep it.
            state.WinningPaths.Add((trace.History, trace.CurrentTime));
        }
    }
}
