while(true)
{
    Console.Write("Enter RPN expression (empty input to quit): ");
    var input = Console.ReadLine();

    if(string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Empty input, exiting.");
        break;
    }

    var tokens = RpnEvaluator.Tokenize(input);
    try
    {
        var result = RpnEvaluator.Evaluate(tokens);
        Console.WriteLine($"Result: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}