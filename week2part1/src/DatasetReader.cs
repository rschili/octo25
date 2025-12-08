public static class DatasetReader
{
    public static List<(uint number, int value)> ReadDataset(string filePath)
    {
        // Read the dataset from file
        string[] lines = File.ReadAllLines(filePath);

        // Parse into list of (uint, int) pairs with validation
        var data = new List<(uint number, int value)>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            // Validate format: should contain " -> "
            if (!line.Contains(" -> "))
            {
                Console.WriteLine($"Warning: Line {i + 1} does not match pattern 'uint -> int': {line}");
                continue;
            }
            
            string[] parts = line.Split(" -> ");
            
            // Validate we have exactly 2 parts
            if (parts.Length != 2)
            {
                Console.WriteLine($"Warning: Line {i + 1} has invalid format: {line}");
                continue;
            }
            
            // Try to parse both values
            if (!uint.TryParse(parts[0].Trim(), out uint number))
            {
                Console.WriteLine($"Warning: Line {i + 1} has invalid uint value: {parts[0]}");
                continue;
            }
            
            if (!int.TryParse(parts[1].Trim(), out int value))
            {
                Console.WriteLine($"Warning: Line {i + 1} has invalid int value: {parts[1]}");
                continue;
            }
            
            data.Add((number, value));
        }

        return data;
    }
}
