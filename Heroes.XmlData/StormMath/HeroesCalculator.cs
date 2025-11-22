using System.Data;

namespace Heroes.XmlData.StormMath;

/// <summary>
/// The Heroes math calculator.
/// </summary>
public sealed class HeroesCalculator
{
    /// <summary>
    /// Gets the maximum number of fractional digits.
    /// </summary>
    internal const int MaxFractionalDigits = 6;

    /// <summary>
    /// Gets the fractional midpoint rounding mode.
    /// </summary>
    internal const MidpointRounding MaxFractionalMidpointRoundingMode = MidpointRounding.ToEven;

    private readonly Stack<double> _values = new();
    private readonly Stack<char> _operators = new();

    private HeroesCalculator()
    {
    }

    /// <summary>
    /// Calculates the math expression.
    /// </summary>
    /// <param name="expression">The math expression.</param>
    /// <returns>The calculated value of the expression.</returns>
    public static double Compute(string expression) => Compute(expression.AsSpan());

    /// <summary>
    /// Calculates the math expression.
    /// </summary>
    /// <param name="expression">The character span that contains the math expression.</param>
    /// <returns>The calculated value of the expression.</returns>
    /// <exception cref="SyntaxErrorException">The expression contains a syntax error.</exception>
    public static double Compute(ReadOnlySpan<char> expression)
    {
        HeroesCalculator heroesMath = new();

        try
        {
            return Math.Round(heroesMath.Calculate(expression), MaxFractionalDigits, MaxFractionalMidpointRoundingMode);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
    }

    internal static double ApplyOperator(char op, double b, double a) => op switch
    {
        '+' => a + b,
        '-' => a - b,
        '*' => a * b,
        '/' => b == 0 ? a : a / b,
        _ => throw new InvalidOperationException($"Invalid operator: {op}"),
    };

    internal static bool IsOperator(char value) => value == '*' || value == '-' || value == '+' || value == '/';

    internal static bool IsDigit(char token)
    {
        if (token >= '0' && token <= '9')
            return true;
        else
            return false;
    }

    // only parentheses has precedence
    // if operator2 has same or higher then return true, otherwise return false
    // only operator2 as operator1 is not used
    private static bool CheckPrecedence(char operator2) => operator2 is not ('(' or ')');

    /* Only parentheses have precedence
     * Divide by 0 results in the numerator
     * With multiple operators in a row, only take the last one
     * account for mismatch parenthesis (too many or too little)
     */
    private double Calculate(ReadOnlySpan<char> tokens)
    {
        char lastReadToken = '\0';

        for (int i = 0; i < tokens.Length; i++)
        {
            // ignore space
            if (tokens[i] == ' ')
                continue;

            // check if digit
            if (IsDigit(tokens[i]) || tokens[i] == '.')
            {
                GetNumber(tokens, ref i, false);
            }
            else if (!IsOperator(lastReadToken) && lastReadToken != ')' && tokens[i] == '-' && !IsDigit(lastReadToken) && i + 1 < tokens.Length && IsDigit(tokens[i + 1])) // negative number check
            {
                if (_operators.Count > 0 && _operators.Peek() != '(' && _values.Count < 1) // no values, then its a negative number
                {
                    _operators.Pop();
                    GetNumber(tokens, ref i, true);
                }
                else
                {
                    GetNumber(tokens, ref i, true);
                }
            }
            else if (tokens[i] == '(')
            {
                _operators.Push(tokens[i]);
            }
            else if (tokens[i] == ')')
            {
                while (_operators.TryPop(out char op) && op != '(')
                {
                    if (op == 'b')
                    {
                        if (_values.TryPop(out double val))
                            _values.Push(val * -1);
                    }
                    else
                    {
                        if (_values.TryPop(out double b) && _values.TryPop(out double a))
                            _values.Push(ApplyOperator(op, b, a));
                    }
                }
            }
            else if (IsOperator(tokens[i]))
            {
                if (IsOperator(lastReadToken))
                {
                    // multiple operators in a row, only take the last one
                    _operators.Pop();
                    _operators.Push(tokens[i]);
                }
                else if (_values.Count == 1 && _operators.Count > 0 && _operators.Peek() == '-')
                {
                    _operators.Pop();
                    _values.Push(_values.Pop() * -1);
                    _operators.Push(tokens[i]);
                }
                else if (tokens[i] == '-' && lastReadToken == '(' && i + 1 < tokens.Length && tokens[i + 1] == '(') // - in between ( and (
                {
                    _operators.Push('b');
                }
                else
                {
                    while (_operators.Count > 0 && CheckPrecedence(_operators.Peek()))
                    {
                        char topOp = _operators.Peek();
                        if (topOp == 'b')
                        {
                            _values.Push(_values.Pop() * -1);
                            _operators.Pop();
                        }
                        else
                        {
                            _values.Push(ApplyOperator(_operators.Pop(), _values.Pop(), _values.Pop()));
                        }
                    }

                    _operators.Push(tokens[i]);
                }
            }

            lastReadToken = tokens[i];
        }

        // pop remaining operators
        while (_operators.Count > 0)
        {
            if (_values.Count == 1 && _operators.Count == 1 && _operators.Peek() == '-')
            {
                _operators.Pop();
                _values.Push(_values.Pop() * -1);
            }
            else
            {
                if (_operators.Peek() == '(')
                    _operators.Pop();
                else if (_values.Count > 1)
                    _values.Push(ApplyOperator(_operators.Pop(), _values.Pop(), _values.Pop()));
                else
                    _operators.Pop();
            }
        }

        while (_values.Count > 1)
        {
            _values.Push(ApplyOperator('+', _values.Pop(), _values.Pop()));
        }

        // final result value
        return _values.Pop();
    }

    private void GetNumber(ReadOnlySpan<char> tokens, ref int i, bool isNegative)
    {
        int beginning = i;

        if (isNegative)
            i++; // negative sign

        bool hasDecimal = false;

        // get entire number
        while (i < tokens.Length)
        {
            if (IsDigit(tokens[i])) // digit
            {
                i++;
            }
            else if (!hasDecimal && tokens[i] == '.') // decimal
            {
                hasDecimal = true;
                i++;
            }
            else
            {
                break;
            }
        }

        _values.Push(double.Parse(tokens[beginning..i]));

        i--;
    }
}
