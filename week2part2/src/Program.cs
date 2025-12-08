// See https://aka.ms/new-console-template for more information

var knownValues = new List<(byte number, int distance)>();

// Read initial 2 pairs
for (int i = 0; i < 2; i++)
{
    knownValues.Add(ReadPair($"Enter pair {i + 1} (byte distance): "));
}

while (true)
{
    var results = HammingDistanceHelper.FindValidNumbers(knownValues);
    
    if (results.Count == 0)
    {
        Console.WriteLine("No results available.");
        return;
    }
    else if (results.Count == 1)
    {
        Console.WriteLine($"Found unique result: {results[0]}");
        return;
    }
    else
    {
        Console.WriteLine($"Found {results.Count} possible results:");
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
        
        knownValues.Add(ReadPair("Enter another pair (byte distance): "));
    }
}

static (byte number, int distance) ReadPair(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Invalid input. Please try again.");
            continue;
        }
        
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !byte.TryParse(parts[0], out byte number) || !int.TryParse(parts[1], out int distance))
        {
            Console.WriteLine("Invalid input format. Expected: <byte> <distance>. Please try again.");
            continue;
        }
        
        return (number, distance);
    }
}
