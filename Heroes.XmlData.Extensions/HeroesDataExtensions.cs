using Heroes.XmlData.StormData;
using System.Xml.Linq;

namespace Heroes.XmlData.Extensions;

/// <summary>
/// Extensions for the <see cref="IHeroesData"/> class.
/// </summary>
public static class HeroesDataExtensions
{
    /// <summary>
    /// Gets the calculated value from an <see cref="XElement"/> with a name of "const" and a <see cref="XAttribute"/> name of "value".
    /// <para>
    /// Example: &lt;const id="$GazloweHeroWeaponDamage" value="100" /&gt; will return 100.
    /// </para>
    /// </summary>
    /// <param name="heroesData">The <see cref="IHeroesData"/>.</param>
    /// <param name="constElement">
    /// The "const" element with a "value" attribute.
    /// <para>
    /// Example: &lt;const value="100" /&gt;.
    /// </para>
    /// </param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromConstElement(this IHeroesData heroesData, XElement constElement)
    {
        string? valueAttribute = constElement.Attribute("value")?.Value;
        string? isExpressionAttribute = constElement.Attribute("evaluateAsExpression")?.Value;

        if (!string.IsNullOrWhiteSpace(valueAttribute) && !string.IsNullOrWhiteSpace(isExpressionAttribute) && isExpressionAttribute == "1")
        {
            return HeroesPrefixNotation.Compute(heroesData, valueAttribute);
        }
        else if (!string.IsNullOrWhiteSpace(valueAttribute) && double.TryParse(valueAttribute, out double value))
        {
            return value;
        }

        return 0;
    }

    /// <summary>
    /// Gets the calculated value from an <see cref="XElement"/> that has a <see cref="XAttribute"/> name of "value".
    /// <para>
    /// Example: &lt;Vital index="Energy" value="50" /&gt; will return 50.
    /// </para>
    /// </summary>
    /// <param name="heroesData">The <see cref="IHeroesData"/>.</param>
    /// <param name="element">
    /// An element with a "value" attribute.
    /// <para>
    /// Example: &lt;Vital value="100" /&gt;.
    /// </para>
    /// </param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromElement(this IHeroesData heroesData, XElement element)
    {
        ReadOnlySpan<char> valueSpan = (element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value).AsSpan().Trim();

        return GetValueFromValueText(heroesData, valueSpan);
    }

    /// <summary>
    /// Gets the calculated value from any text (preferred from a "value" attribute).
    /// </summary>
    /// <param name="heroesData">The <see cref="IHeroesData"/>.</param>
    /// <param name="value">Text from a "value" attribute.</param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromValueText(this IHeroesData heroesData, string value)
    {
        return GetValueFromValueText(heroesData, value.AsSpan());
    }

    /// <summary>
    /// Gets the calculated value from any text (preferred from a "value" attribute).
    /// </summary>
    /// <param name="heroesData">The <see cref="IHeroesData"/>.</param>
    /// <param name="value">A character span that contains the text from a "value" attribute.</param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromValueText(this IHeroesData heroesData, ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
            return 0;

        if (value[0] == '$')
        {
            if (heroesData.TryGetConstantElement(value, out StormXElementValue? stormXElementValue))
            {
                return heroesData.GetValueFromConstElement(stormXElementValue.Value);
            }
        }
        else if (double.TryParse(value, out double result))
        {
            return result;
        }

        return 0;
    }
}
