using System.Data;

namespace Heroes.XmlData.StormMath;

internal sealed class HeroesPrefixNotation
{
    private readonly IStormStorage _stormStorage;

    private HeroesPrefixNotation(IStormStorage stormStorage)
    {
        _stormStorage = stormStorage;
    }

    public static double Compute(IStormStorage stormStorage, ReadOnlySpan<char> expression)
    {
        HeroesPrefixNotation heroesPrefixNotation = new(stormStorage);

        try
        {
            return Math.Round(heroesPrefixNotation.Evaluate(expression), HeroesCalculator.MaxFractionalDigits, HeroesCalculator.MaxFractionalMidpointRoundingMode);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
    }

    private static ReadOnlySpan<char> GetExpression(ReadOnlySpan<char> expression)
    {
        int startIndex = expression.IndexOf('(');

        if (startIndex == -1)
            return expression;

        int parenthesis = 1;

        for (int i = startIndex + 1; i < expression.Length; i++)
        {
            if (expression[i] == '(')
            {
                parenthesis++;
            }
            else if (expression[i] == ')')
            {
                parenthesis--;

                if (parenthesis == 0)
                {
                    return expression.Slice(startIndex + 1, i - startIndex - 1);
                }
            }
        }

        return expression;
    }

    private static int GetSplitIndex(ReadOnlySpan<char> expression)
    {
        int parenthesis = 0;

        for (int i = 0; i < expression.Length; i++)
        {
            if (expression[i] == '(')
            {
                parenthesis++;
            }
            else if (expression[i] == ')')
            {
                parenthesis--;
            }
            else if (parenthesis == 0 && expression[i] == ' ')
            {
                return i;
            }
        }

        return 0;
    }

    private double Evaluate(ReadOnlySpan<char> expression)
    {
        char firstChar = expression[0];

        if (HeroesCalculator.IsOperator(firstChar) && expression.Length > 1 && expression[1] == '(')
        {
            char op = firstChar;

            GetOperatorParameters(expression, out double firstParam, out double secondParam);

            return HeroesCalculator.ApplyOperator(op, secondParam, firstParam);
        }
        else if (firstChar == '$')
        {
            return _stormStorage.GetValueFromConstTextAsNumber(expression);
        }
        else if (double.TryParse(expression, out double value))
        {
            return value;
        }
        else if (expression.StartsWith("negate", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> valueToBeNegatedSpan = expression[7..^1];  // removed negate( and )

            return Evaluate(valueToBeNegatedSpan) * -1;
        }
        else if (expression.StartsWith("max", StringComparison.OrdinalIgnoreCase))
        {
            GetOperatorParameters(expression, out double firstParam, out double secondParam);

            return Math.Max(firstParam, secondParam);
        }
        else if (expression.StartsWith("min", StringComparison.OrdinalIgnoreCase))
        {
            GetOperatorParameters(expression, out double firstParam, out double secondParam);

            return Math.Min(firstParam, secondParam);
        }

        return 0;
    }

    private void GetOperatorParameters(ReadOnlySpan<char> expression, out double firstParam, out double secondParam)
    {
        ReadOnlySpan<char> currentExpression = GetExpression(expression);
        int indexSplit = GetSplitIndex(currentExpression);

        firstParam = Evaluate(currentExpression[..indexSplit]);
        secondParam = Evaluate(currentExpression[(indexSplit + 1)..]);
    }
}
