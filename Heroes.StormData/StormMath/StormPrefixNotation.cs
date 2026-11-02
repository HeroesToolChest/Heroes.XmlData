using System.Data;

namespace Heroes.StormData.StormMath;

internal static class StormPrefixNotation
{
    public static double Compute(LayeredDataStorage layeredDataStorage, ReadOnlySpan<char> expression)
    {
        try
        {
            return Math.Round(Evaluate(layeredDataStorage, expression), StormCalculator.MaxFractionalDigits, StormCalculator.MaxFractionalMidpointRoundingMode);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
    }

    private static double Evaluate(LayeredDataStorage layeredDataStorage, ReadOnlySpan<char> expression)
    {
        char firstChar = expression[0];

        if (StormCalculator.IsOperator(firstChar) && expression.Length > 1 && expression[1] == '(')
        {
            GetParameters(layeredDataStorage, expression, 1, out double firstParam, out double secondParam);

            return StormCalculator.ApplyOperator(firstChar, secondParam, firstParam);
        }
        else if (firstChar == '$')
        {
            if (layeredDataStorage.TryGetFirstStormXmlConstElementById(expression, out StormXmlConstElement? stormXmlConstElement))
                return ParseConstValue(stormXmlConstElement.Value);
            else
                return 0;
        }

        // plain decimals take the fast path, functions are checked before the slower double.TryParse, which is left for other number forms
        if (StormCalculator.TryParseExactDecimal(expression, out double value))
        {
            return value;
        }
        else if (expression.StartsWith("negate", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> valueToBeNegatedSpan = expression[7..^1];  // removed negate( and )

            return Evaluate(layeredDataStorage, valueToBeNegatedSpan) * -1;
        }
        else if (expression.StartsWith("round", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> valueToBeRoundedSpan = expression[6..^1];  // removed round( and )

            return Math.Round(Evaluate(layeredDataStorage, valueToBeRoundedSpan));
        }
        else if (expression.StartsWith("max", StringComparison.OrdinalIgnoreCase))
        {
            GetParameters(layeredDataStorage, expression, expression.IndexOf('('), out double firstParam, out double secondParam);

            return Math.Max(firstParam, secondParam);
        }
        else if (expression.StartsWith("min", StringComparison.OrdinalIgnoreCase))
        {
            GetParameters(layeredDataStorage, expression, expression.IndexOf('('), out double firstParam, out double secondParam);

            return Math.Min(firstParam, secondParam);
        }
        else if (double.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) // other number forms, e.g. 1e5
        {
            return value;
        }

        return 0;
    }

    // Splits the two space separated parameters inside the parentheses that open at openIndex, e.g. +(*(2 3) 4) into *(2 3) and 4.
    // Finds the split and the closing parenthesis in a single pass.
    private static void GetParameters(LayeredDataStorage layeredDataStorage, ReadOnlySpan<char> expression, int openIndex, out double firstParam, out double secondParam)
    {
        if (openIndex < 0)
            throw new FormatException("Missing an opening parenthesis.");

        int splitIndex = -1;
        int parenthesis = 0;

        for (int i = openIndex + 1; i < expression.Length; i++)
        {
            char token = expression[i];

            if (token == '(')
            {
                parenthesis++;
            }
            else if (token == ')')
            {
                if (parenthesis == 0)
                {
                    // the first parameter can't be empty
                    if (splitIndex <= openIndex + 1)
                        throw new FormatException("Expected two parameters separated by a space.");

                    firstParam = Evaluate(layeredDataStorage, expression[(openIndex + 1)..splitIndex]);
                    secondParam = Evaluate(layeredDataStorage, expression[(splitIndex + 1)..i]);

                    return;
                }

                parenthesis--;
            }
            else if (token == ' ' && parenthesis == 0 && splitIndex < 0)
            {
                splitIndex = i;
            }
        }

        throw new FormatException("Missing a closing parenthesis.");
    }

    // same result as double.Parse(value, CultureInfo.InvariantCulture), with plain decimals taking the fast path
    private static double ParseConstValue(string value)
    {
        if (StormCalculator.TryParseExactDecimal(value, out double result))
            return result;

        return double.Parse(value, CultureInfo.InvariantCulture);
    }
}
