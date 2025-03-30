namespace Heroes.XmlData.StormMath;

internal static class MathHelpers
{
    /// <summary>
    /// Gets the count of digits in a number. Includes the fractional, decimal, and negative sign.
    /// </summary>
    /// <param name="number">The number to evaluate.</param>
    /// <returns>The number of digits.</returns>
    public static int GetCountOfDigits(double number)
    {
        if (number == 0)
            return 1;

        int count = 0;

        if (double.IsNegative(number))
        {
            count++;
            number = number * -1;
        }

        // should technically be a long, but we shouldn't be getting that large a number anyway
        int integerPart = (int)number;

        do
        {
            count++;
            integerPart /= 10;
        }
        while (integerPart > 0);

        double fractionPart = number - (int)number;
        if (fractionPart > 0)
            count++; // count decimal point

        int maxPrecision = HeroesCalculator.MaxFractionalDigits;

        while (fractionPart > 0 && maxPrecision-- > 0)
        {
            fractionPart *= 10;
            int digit = (int)fractionPart;
            count++;

            fractionPart -= digit;

            if (fractionPart < 1e-6)
                break;
        }

        return count;
    }
}
