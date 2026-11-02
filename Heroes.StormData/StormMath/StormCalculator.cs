using System.Data;

namespace Heroes.StormData.StormMath;

/// <summary>
/// The Heroes math calculator.
/// </summary>
public static class StormCalculator
{
    /// <summary>
    /// Gets the maximum number of fractional digits.
    /// </summary>
    internal const int MaxFractionalDigits = 6;

    /// <summary>
    /// Gets the fractional midpoint rounding mode.
    /// </summary>
    internal const MidpointRounding MaxFractionalMidpointRoundingMode = MidpointRounding.ToEven;

    /// <summary>
    /// Gets the longest number that can be written: a negative sign, 6 whole digits (<see cref="ValueScale.MaxValueSize"/>),
    /// a decimal point and 6 decimal places, e.g. -999998.123456.
    /// Numbers up to this length are parsed by a fast path that is only exact for up to 15 digits, so this must stay at or below 15.
    /// </summary>
    internal const int MaxNumberLength = 14;

    // expressions up to this length keep their stacks on the stack (256 doubles is 2 KB), longer ones rent from the array pool
    private const int MaxStackAllocLength = 256;

    private static ReadOnlySpan<double> PowersOf10 =>
    [
        1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15,
    ];

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
        int capacity = expression.Length;

        double[]? rentedValues = null;
        char[]? rentedOperators = null;

        Span<double> valuesBuffer = capacity <= MaxStackAllocLength
            ? stackalloc double[capacity]
            : (rentedValues = ArrayPool<double>.Shared.Rent(capacity));

        Span<char> operatorsBuffer = capacity <= MaxStackAllocLength
            ? stackalloc char[capacity]
            : (rentedOperators = ArrayPool<char>.Shared.Rent(capacity));

        try
        {
            Evaluator evaluator = new(valuesBuffer, operatorsBuffer);

            return Math.Round(evaluator.Calculate(expression), MaxFractionalDigits, MaxFractionalMidpointRoundingMode);
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Syntax error in expression: {expression}", ex);
        }
        finally
        {
            if (rentedValues is not null)
                ArrayPool<double>.Shared.Return(rentedValues);

            if (rentedOperators is not null)
                ArrayPool<char>.Shared.Return(rentedOperators);
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

    internal static bool IsOperator(char value) => value is '*' or '-' or '+' or '/';

    internal static bool IsDigit(char token) => char.IsAsciiDigit(token);

    /// <summary>
    /// Parses a number, giving the same result as <see cref="double.Parse(ReadOnlySpan{char}, NumberStyles, IFormatProvider?)"/>
    /// with <see cref="NumberStyles.Float"/> and the invariant culture.
    /// </summary>
    /// <param name="number">The number to parse.</param>
    /// <returns>The parsed number.</returns>
    /// <exception cref="FormatException"><paramref name="number"/> is not a number.</exception>
    internal static double ParseNumber(ReadOnlySpan<char> number)
    {
        if (TryParseExactDecimal(number, out double value))
            return value;

        return double.Parse(number, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    // Fast path for plain decimals like -12.375, the only kind of number in an expression.
    // A number up to MaxNumberLength characters has at most 14 digits, which fit exactly in a double, as does every power of ten
    // it can be divided by, and division is rounded correctly, so dividing one by the other gives the same result as double.Parse.
    internal static bool TryParseExactDecimal(ReadOnlySpan<char> number, out double value)
    {
        value = 0;

        if (number.Length > MaxNumberLength)
            return false;

        bool isNegative = !number.IsEmpty && number[0] == '-';
        if (isNegative)
            number = number[1..];

        long mantissa = 0;
        int fractionalDigitCount = 0;
        bool hasDigit = false;
        bool hasDecimal = false;

        foreach (char token in number)
        {
            if (IsDigit(token))
            {
                mantissa = (mantissa * 10) + (token - '0');
                hasDigit = true;

                if (hasDecimal)
                    fractionalDigitCount++;
            }
            else if (token == '.' && !hasDecimal)
            {
                hasDecimal = true;
            }
            else
            {
                return false;
            }
        }

        // no digits, e.g. "." is left for double.Parse to reject
        if (!hasDigit)
            return false;

        value = mantissa / PowersOf10[fractionalDigitCount];

        if (isNegative)
            value = -value;

        return true;
    }

    // only parentheses has precedence
    // if operator2 has same or higher then return true, otherwise return false
    // only operator2 as operator1 is not used
    private static bool CheckPrecedence(char operator2) => operator2 is not ('(' or ')');

    // a fixed-size stack over a caller-provided buffer, so a calculation doesn't allocate
    private ref struct SpanStack<T>
    {
        private readonly Span<T> _buffer;

        public SpanStack(Span<T> buffer)
        {
            _buffer = buffer;
            Count = 0;
        }

        public int Count { get; private set; }

        public void Push(T item) => _buffer[Count++] = item;

        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException("Stack empty.");

            return _buffer[--Count];
        }

        public readonly T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("Stack empty.");

            return _buffer[Count - 1];
        }

        public bool TryPop(out T item)
        {
            if (Count == 0)
            {
                item = default!;
                return false;
            }

            item = _buffer[--Count];
            return true;
        }
    }

    private ref struct Evaluator
    {
        private SpanStack<double> _values;
        private SpanStack<char> _operators;

        public Evaluator(Span<double> valuesBuffer, Span<char> operatorsBuffer)
        {
            _values = new SpanStack<double>(valuesBuffer);
            _operators = new SpanStack<char>(operatorsBuffer);
        }

        /* Only parentheses have precedence
         * Divide by 0 results in the numerator
         * With multiple operators in a row, only take the last one
         * account for mismatch parenthesis (too many or too little)
         */
        public double Calculate(ReadOnlySpan<char> tokens)
        {
            char lastReadToken = '\0';

            for (int i = 0; i < tokens.Length; i++)
            {
                char token = tokens[i];

                // ignore space
                if (token == ' ')
                    continue;

                // check if digit
                if (IsDigit(token) || token == '.')
                {
                    GetNumber(tokens, ref i, false);
                }
                else if (token == '-' && !IsOperator(lastReadToken) && lastReadToken != ')' && !IsDigit(lastReadToken) && i + 1 < tokens.Length && IsDigit(tokens[i + 1])) // negative number check
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
                else if (token == '(')
                {
                    _operators.Push(token);
                }
                else if (token == ')')
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
                else if (IsOperator(token))
                {
                    if (IsOperator(lastReadToken))
                    {
                        // multiple operators in a row, only take the last one
                        _operators.Pop();
                        _operators.Push(token);
                    }
                    else if (_values.Count == 1 && _operators.Count > 0 && _operators.Peek() == '-')
                    {
                        _operators.Pop();
                        _values.Push(_values.Pop() * -1);
                        _operators.Push(token);
                    }
                    else if (token == '-' && lastReadToken == '(' && i + 1 < tokens.Length && tokens[i + 1] == '(') // - in between ( and (
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

                        _operators.Push(token);
                    }
                }

                // GetNumber moves i to the last character of the number
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
                char token = tokens[i];

                if (IsDigit(token)) // digit
                {
                    i++;
                }
                else if (!hasDecimal && token == '.') // decimal
                {
                    hasDecimal = true;
                    i++;
                }
                else
                {
                    break;
                }
            }

            _values.Push(ParseNumber(tokens[beginning..i]));

            i--;
        }
    }
}
