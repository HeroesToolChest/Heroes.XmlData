using System.Data;

namespace Heroes.XmlData.StormMath;

/// <summary>
/// The Heroes math calculator.
/// </summary>
public class HeroesCalculator
{
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
            return heroesMath.Calculate(expression);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
    }

    internal static double ApplyOperator(char op, double b, double a)
    {
        switch (op)
        {
            case '+':
                return a + b;
            case '-':
                return a - b;
            case '*':
                return a * b;
            case '/':
                if (b == 0)
                {
                    // if divide by 0, return the numerator
                    return a;
                }

                return a / b;
        }

        throw new InvalidOperationException($"Invalid operator: {op}");
    }

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
#pragma warning disable IDE0060 // Remove unused parameter
    private static bool CheckPrecedence(char operator1, char operator2)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (operator2 == '(' || operator2 == ')')
            return false;
        ////else if ((operator1 == '*' || operator1 == '/') && (operator2 == '+' || operator2 == '-'))
        ////   return false;
        else
            return true;
    }

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
                while (_operators.Count > 0 && _operators.Peek() != '(')
                {
                    if (_operators.Peek() == 'b')
                    {
                        _values.Push(_values.Pop() * -1);
                        _operators.Pop();
                    }
                    else
                    {
                        _values.Push(ApplyOperator(_operators.Pop(), _values.Pop(), _values.Pop()));
                    }
                }

                if (_operators.Count > 0)
                    _operators.Pop(); // this pops the left parentheses
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
                    while (_operators.Count > 0 && CheckPrecedence(tokens[i], _operators.Peek()))
                    {
                        if (_operators.Peek() == 'b')
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
