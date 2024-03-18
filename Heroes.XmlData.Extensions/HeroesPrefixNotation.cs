using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Heroes.XmlData.Extensions;

internal class HeroesPrefixNotation
{
    private readonly IHeroesData _heroesData;
    private int _index = 0;

    private HeroesPrefixNotation(IHeroesData heroesData)
    {
        _heroesData = heroesData;
    }

    public static double Compute(IHeroesData heroesData, ReadOnlySpan<char> expression)
    {
        HeroesPrefixNotation heroesPrefixNotation = new(heroesData);

        try
        {
            return heroesPrefixNotation.Evaluate(expression);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
    }

    private double Evaluate(ReadOnlySpan<char> expression)
    {
        if (!TryGetOperator(expression, out char? op))
            throw new SyntaxErrorException("Operator not found");

        double? firstValue = GetExpression(expression, '(', ' ');
        double? secondValue = GetExpression(expression, ' ', ')');

        if (!firstValue.HasValue && !secondValue.HasValue)
            return 0;

        return HeroesMath.ApplyOperator(op.Value, secondValue.Value, firstValue.Value);
    }

    private bool TryGetOperator(ReadOnlySpan<char> expression, [NotNullWhen(true)] out char? op)
    {
        op = null;

        bool found = HeroesMath.IsOperator(expression[_index]);

        if (found)
        {
            op = expression[_index];

            return true;
        }

        return false;
    }

    private double GetExpression(ReadOnlySpan<char> expression, char startChar, char endChar)
    {
        ReadOnlySpan<char> currentTextSpan = expression[_index..];

        int startIndex = currentTextSpan.IndexOf(startChar);

        if (startIndex < 0)
            return 0;

        for (int i = startIndex; i < currentTextSpan.Length; i++)
        {
            if (HeroesMath.IsOperator(currentTextSpan[i]))
            {
                double value = Evaluate(currentTextSpan[(startIndex + 1)..]);

                _index += i + 1; // 1 for )
                return value;
            }

            if (currentTextSpan[i] == endChar)
            {
                _index += i;
                ReadOnlySpan<char> value = currentTextSpan.Slice(startIndex + 1, i - startIndex - 1);

                return _heroesData.GetValueFromValueText(value);
            }
        }

        return 0;
    }
}
