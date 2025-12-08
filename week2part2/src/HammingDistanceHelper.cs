using System.Numerics;

public static class HammingDistanceHelper
{
    public static int HammingDistance(byte a, byte b)
    {
        // XOR the numbers and then count the number of set bits
        return BitOperations.PopCount((uint)(a ^ b));
    }

    /// <summary>
    /// Generates all 8-bit integers that have a specific Hamming distance from the seed.
    /// Uses Gosper's Hack for zero-allocation, high-speed bit permutation.
    /// </summary>
    /// <param name="seed">The starting number.</param>
    /// <param name="distance">The number of bits to flip (k).</param>
    public static IEnumerable<byte> GetVariationsGosper(byte seed, int distance)
    {
        // Edge cases
        if (distance == 0) { yield return seed; yield break; }
        if (distance >= 8) { yield return (byte)~seed; yield break; }

        // 1. Create the lexicographically first bitmask with 'distance' bits set.
        // Example if distance is 3: 000...00111
        uint current = (1u << distance) - 1;

        // Loop until we overflow 8 bits
        while (true)
        {
            // Yield the variation (Seed XOR Mask)
            yield return (byte)(seed ^ current);

            // --- Gosper's Hack Step-by-Step ---

            // 2. Find the lowest set bit (c)
            // Example: 001110 -> 000010
            uint c = current & (uint)-(int)current;

            // 3. Ripple carry to the left (r)
            // Example: 001110 + 000010 -> 010000
            uint r = current + c;

            // 4. Termination Check: If r is 0 or exceeds 8 bits, we have finished all permutations
            if (r == 0 || r > 0xFF) break;

            // 5. Calculate the Next Permutation
            // Classic Formula: next = (((r ^ current) >> 2) / c) | r

            // OPTIMIZATION for .NET:
            // Division is expensive (10-20 cycles). 
            // Since 'c' is a power of 2, we can use a right shift based on trailing zeros.
            // This reduces the cost to ~1 cycle.
            int shiftAmount = BitOperations.TrailingZeroCount(c);

            // Perform the "division" via shift
            uint rightPart = ((r ^ current) >> 2) >> shiftAmount;

            current = rightPart | r;
        }
    }

    public static List<byte> FindValidNumbers(List<(byte number, int distance)> knownValues)
    {
        // find entry with lowest distance
        var bestEntry = knownValues.OrderBy(e => e.distance).First();
        byte baseNumber = bestEntry.number;
        int baseDistance = bestEntry.distance;
        var variations = GetVariationsGosper(baseNumber, baseDistance);

        // Thread-safe collection for results
        var validNumbers = new System.Collections.Concurrent.ConcurrentBag<byte>();
        
        // Cancellation token to stop early when we find 100+ results
        var cts = new CancellationTokenSource();

        try
        {
            Parallel.ForEach(
                variations,
                new ParallelOptions { CancellationToken = cts.Token },
                (variation, state) =>
                {
                    // Check if we already have enough results
                    if (validNumbers.Count >= 100)
                    {
                        cts.Cancel();
                        state.Stop();
                        return;
                    }

                    // Validate this variation against all known values
                    foreach (var (number, expectedDistance) in knownValues)
                    {
                        int actualDistance = HammingDistance(variation, number);
                        if (actualDistance != expectedDistance)
                        {
                            return; // Invalid, skip to next variation
                        }
                    }

                    // If we get here, all distances matched - this variation is valid
                    validNumbers.Add(variation);
                });
        }
        catch (OperationCanceledException)
        {
            // Expected when we hit our limit
        }

        return validNumbers.ToList();
    }
}