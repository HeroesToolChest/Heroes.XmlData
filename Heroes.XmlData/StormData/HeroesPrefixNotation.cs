using System.Data;

namespace Heroes.XmlData.StormData;

internal class HeroesPrefixNotation
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
            return heroesPrefixNotation.Evaluate(expression);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
    }

    private static ReadOnlySpan<char> GetExpression(ReadOnlySpan<char> expression)
    {
        int startIndex = expression.IndexOf('(');

        int parenthesis = 0;

        if (startIndex > -1)
            parenthesis++;
        else
            return expression;

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
                return i++;
            }
        }

        return 0;
    }

    private double Evaluate(ReadOnlySpan<char> expression)
    {
        if (HeroesMath.IsOperator(expression[0]) && expression.Length > 1 && expression[1] == '(')
        {
            char op = expression[0];

            ReadOnlySpan<char> currentExpression = GetExpression(expression);

            int indexSplit = GetSplitIndex(currentExpression);

            ReadOnlySpan<char> firstValueSpan = currentExpression[0..indexSplit];
            double firstValue = Evaluate(firstValueSpan);

            ReadOnlySpan<char> secondValueSpan = currentExpression.Slice(indexSplit + 1, currentExpression.Length - indexSplit - 1);
            double secondValue = Evaluate(secondValueSpan);

            return HeroesMath.ApplyOperator(op, secondValue, firstValue);
        }
        else if (expression[0] == '$')
        {
            return _stormStorage.GetValueFromConstTextAsNumber(expression);
        }
        else if (double.TryParse(expression, out double value))
        {
            return value;
        }
        else if (expression.StartsWith("negate"))
        {
            ReadOnlySpan<char> valueToBeNegatedSpan = expression[6..].Trim("()");

            if (double.TryParse(valueToBeNegatedSpan, out value))
                return value * -1;
        }

        return 0;
    }
}
