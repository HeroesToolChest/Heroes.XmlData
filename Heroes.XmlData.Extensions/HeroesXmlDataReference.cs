namespace Heroes.XmlData.Extensions;

internal class HeroesXmlDataReference
{
    private readonly HeroesData _heroesData;

    private HeroesXmlDataReference(HeroesData heroesData)
    {
        _heroesData = heroesData;
    }

    public static string Get(HeroesData heroesData, ReadOnlySpan<char> textRef)
    {
        HeroesXmlDataReference heroesXmlDataReference = new(heroesData);

        heroesXmlDataReference.Test(textRef);
        return "";
    }

    private void Test(ReadOnlySpan<char> textRef)
    {
        int totalSplitCount = textRef.Count('.');
        totalSplitCount += textRef.Count(',');

        // each parts of the reference
        Span<Range> refParts = stackalloc Range[totalSplitCount];

        textRef.Trim().SplitAny(refParts, ".,", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        ReadOnlySpan<char> firstPart = textRef[refParts[0]];

        if (firstPart[0] == '$')
        {
            // is constant
        }
        else if (refParts.Length > 2)
        {
            Test2(refParts);
        }

    }

    private void Test2(Span<Range> refParts)
    {
        //_heroesData.GetElements
    }
}
