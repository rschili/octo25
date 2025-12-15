using System;
using System.Collections.Generic;
using System.Linq;

public static class RpnEvaluator
{
    public static IEnumerable<string> Tokenize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Enumerable.Empty<string>();

        return input.Split([' '], StringSplitOptions.RemoveEmptyEntries);
    }

    public static int Evaluate(string input)
    {
        return Evaluate(Tokenize(input));
    }

    public static int Evaluate(IEnumerable<string> tokens)
    {
        var stack = new Stack<int>();

        foreach (var token in tokens)
        {
            // If it's a number, push it to the stack
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
                continue;
            }

            // It's an operator; pop the required operands
            if (stack.Count < 2) 
                throw new InvalidOperationException("Expression invalid: insufficient operands.");

            int right = stack.Pop();
            int left = stack.Pop();

            int result = token switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" => left / right,
                _   => throw new ArgumentException($"Unknown operator: {token}")
            };

            stack.Push(result);
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Expression invalid: too many operands remaining.");

        return stack.Pop();
    }
}