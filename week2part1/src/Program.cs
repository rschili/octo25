// See https://aka.ms/new-console-template for more information

// Read the dataset from Dataset.txt
var data = DatasetReader.ReadDataset("Dataset.txt");

Console.WriteLine($"Successfully loaded {data.Count} entries from Dataset.txt");
if (data.Count == 0)
{
    Console.WriteLine("No data entries found in Dataset.txt");
    return;
}

var actualResult = HammingDistanceHelper.FindValidNumbers(data);
Console.WriteLine($"Found {actualResult.Count} valid numbers.");
foreach (var number in actualResult)
{
    Console.WriteLine(number);
}
