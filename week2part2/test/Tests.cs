namespace HammingTests;

public record HammingDistanceTestCase(byte Number, byte OtherNumber, int ExpectedDistance);

public class Tests
{

    public static IEnumerable<Func<HammingDistanceTestCase>> GetTestCases()
    {
        yield return () => new HammingDistanceTestCase(3, 6, 2);
        yield return () => new HammingDistanceTestCase(0, 0, 0);
        yield return () => new HammingDistanceTestCase(3, 7, 1);
        yield return () => new HammingDistanceTestCase(3, 4, 3);
        yield return () => new HammingDistanceTestCase(4, 3, 3);
    }

    [Test]
    [MethodDataSource(nameof(GetTestCases))]
    public async Task TestHammingDistanceCalculation(HammingDistanceTestCase testCase)
    {
        await Assert.That(HammingDistanceHelper.HammingDistance(testCase.Number, testCase.OtherNumber))
            .IsEqualTo(testCase.ExpectedDistance);
    }

    private long CalculateCombinations(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0 || k == n) return 1;
        
        // Optimization: k and n-k are symmetric. Use the smaller one to loop less.
        if (k > n / 2) k = n - k;

        long result = 1;
        for (int i = 1; i <= k; i++)
        {
            // Standard way to compute combinations iteratively:
            // result = result * (n - i + 1) / i
            // We use BigInteger logic implicitly by doing multiply first, then divide.
            // (Checked context ensures we catch overflows, though long is enough for 32 bits)
            checked
            {
                result = result * (n - i + 1) / i;
            }
        }
        return result;
    }

    [Test]
    public async Task TestGetVariationGosperCount()
    {
        // Settings
        byte seed = 0; // The value of seed doesn't affect the COUNT, only the values.
        int totalBits = 8;

        for(int i = 1; i < 5; i++)
        {
            int distance = i;

            // Act
            var variations = HammingDistanceHelper.GetVariationsGosper(seed, distance);
            long actualCount = variations.LongCount();

            // Assert
            long expectedCount = CalculateCombinations(totalBits, distance);
            await Assert.That(actualCount).IsEqualTo(expectedCount);
        }
    }

    [Test]
    public async Task TestGetVariationRoundtrip()
    {
        byte seed = (byte)Random.Shared.Next(0, 256);
        for(int distance = 0; distance <= 5; distance++)
        {
            var variations = HammingDistanceHelper.GetVariationsGosper(seed, distance);
            foreach(var variation in variations)
            {
                int actualDistance = HammingDistanceHelper.HammingDistance(seed, variation);
                await Assert.That(actualDistance).IsEqualTo(distance);
            }
        }
    }

    [Test]
    public async Task TestFindValidNumbersForSampleDataset()
    {
        // Sample known values
        var knownValues = new List<(byte number, int distance)>
        {
            (6, 2),
            (7, 1),
            (4, 3)
        };
        //The method works with 32 bit inputs while the sample is for 3 bit uints, so we'll just use the lowest result for validation

        byte expectedResult = 3;
        var actualResult = HammingDistanceHelper.FindValidNumbers(knownValues);
        var lowestResult = actualResult.Min();
        await Assert.That(lowestResult).IsEqualTo(expectedResult);
    }
}
