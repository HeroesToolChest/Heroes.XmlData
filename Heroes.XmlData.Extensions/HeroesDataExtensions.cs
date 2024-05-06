using Heroes.XmlData.StormData;
using System.Xml.Linq;

namespace Heroes.XmlData.Extensions;

/// <summary>
/// Extensions for the <see cref="HeroesData"/> class.
/// </summary>
public static class HeroesDataExtensions
{
    /// <summary>
    /// Gets the calculated value from an <see cref="XElement"/> with a name of <c>const</c> and a <see cref="XAttribute"/> <c>value</c>.
    /// If there is a <see cref="XAttribute"/> <c>evaluateAsExpression</c> with a value of <c>1</c>, then then attribute <c>value</c> will be evalulated as an expression.
    /// <para>
    /// Example: &lt;const id="$GazloweHeroWeaponDamage" value="100" /&gt; will return 100.
    /// </para>
    /// <para>
    /// Example: &lt;const id="$GazloweDethLazorFirinMahLazorzWarningRange" value="/($GazloweDethLazorFirinMahLazorzRange 32)" evaluateAsExpression="1" /&gt;
    /// will be evaluated an an expression and return the calculated value.
    /// </para>
    /// </summary>
    /// <param name="heroesData">The <see cref="HeroesData"/>.</param>
    /// <param name="constElement">The <c>const</c> element with a <c>value</c> attribute and optional <c>evaluateAsExpression</c> attribute.
    /// </param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromConstElement(this HeroesData heroesData, XElement constElement)
    {
        string? valueAttribute = constElement.Attribute("value")?.Value;
        string? isExpressionAttribute = constElement.Attribute("evaluateAsExpression")?.Value;

        if (!string.IsNullOrWhiteSpace(valueAttribute) && !string.IsNullOrWhiteSpace(isExpressionAttribute) && isExpressionAttribute == "1")
        {
            return 0;//HeroesPrefixNotation.Compute(heroesData, valueAttribute);
        }
        else if (!string.IsNullOrWhiteSpace(valueAttribute) && double.TryParse(valueAttribute, out double value))
        {
            return value;
        }

        return 0;
    }

    /// <summary>
    /// Gets the calculated value from any <see cref="XElement"/> that has a <see cref="XAttribute"/> <c>value</c>.
    /// <para>
    /// Example: &lt;Vital index="Energy" value="50" /&gt; will return 50.
    /// </para>
    /// </summary>
    /// <param name="heroesData">The <see cref="HeroesData"/>.</param>
    /// <param name="element">An element with a <c>value</c> attribute.</param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromElement(this HeroesData heroesData, XElement element)
    {
        ReadOnlySpan<char> valueSpan = (element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value).AsSpan().Trim();

        return GetValueFromValueText(heroesData, valueSpan);
    }

    /// <summary>
    /// Gets the calculated value from any text.
    /// </summary>
    /// <param name="heroesData">The <see cref="HeroesData"/>.</param>
    /// <param name="text">Text from a <c>value</c> attribute.</param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromValueText(this HeroesData heroesData, string text)
    {
        return GetValueFromValueText(heroesData, text.AsSpan());
    }

    /// <summary>
    /// Gets the calculated value from any text.
    /// </summary>
    /// <param name="heroesData">The <see cref="HeroesData"/>.</param>
    /// <param name="text">A character span that contains the text from a <c>value</c> attribute.</param>
    /// <returns>The calculated value.</returns>
    public static double GetValueFromValueText(this HeroesData heroesData, ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return 0;

        if (text[0] == '$')
        {
            if (heroesData.TryGetConstantXElement(text, out StormXElementValuePath? stormXElementValue))
            {
                return heroesData.GetValueFromConstElement(stormXElementValue.Value);
            }
        }
        else if (double.TryParse(text, out double result))
        {
            return result;
        }

        return 0;
    }

    public static string TestGetSomeValue(this HeroesData heroesData, ReadOnlySpan<char> textRef)
    {
        HeroesXmlDataReference.Get(heroesData, textRef);


        return "";
    }
}
