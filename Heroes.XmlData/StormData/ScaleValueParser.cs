using Heroes.XmlData.GameStrings;

namespace Heroes.XmlData.StormData;

internal static class ScaleValueParser
{
    public const string ScaleAttributeName = $"{HxdConstants.Name}Scale";

    public static StormElement? CreateStormElement(StormStorage stormStorage, LevelScalingEntry levelScalingEntry, StormStringValue stormStringValue)
    {
        // field from the internal storm element data which might be different from the level scaling entry field but will be more "accurate"
        string? validatedField = GetValidatedField(stormStorage, levelScalingEntry);

        if (validatedField is not null)
        {
            ReadOnlySpan<char> fieldSpan = validatedField;
            int splitterCount = fieldSpan.Count('.');

            Span<Range> fieldParts = stackalloc Range[splitterCount + 1];

            fieldSpan.Split(fieldParts, '.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // builds an XElement from the internal field
            XElement newScalingElement = CreateXElement(levelScalingEntry, stormStringValue, fieldSpan, fieldParts);

            // creates a storm element from the internal field
            StormElement stormScalingElement = new(new StormXElementValuePath(newScalingElement, stormStringValue.StormPath));

            return stormScalingElement;
        }

        return null;
    }

    private static string? GetValidatedField(StormStorage stormStorage, LevelScalingEntry levelScalingEntry)
    {
        ReadOnlySpan<char> fieldSpan = levelScalingEntry.Field;
        int splitterCount = fieldSpan.Count('.');

        Span<Range> fieldParts = stackalloc Range[splitterCount + 1];

        fieldSpan.Split(fieldParts, '.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // check if the entry exists and get the storm element
        StormElement? completeStormElement = stormStorage.GetCompleteStormElement(levelScalingEntry.Entry, levelScalingEntry.Catalog);

        // doesn't exist
        if (completeStormElement is null)
            return null;

        // check if the field exists
        StormElementData lastData = DataRefParser.GetStormElementDataFromLastFieldPart(completeStormElement.DataValues, levelScalingEntry.Field, fieldParts);

        // if it has a value, then we found the value for the scaling entry
        if (lastData.HasValue || lastData.HasConstValue)
        {
            // return the field from the storm element, this might be different from the given scaling entry but will be more "accurate"
            return lastData.Field;
        }

        return null;
    }

    // builds an XElement from the the field given along with the scale value from the level scaling entry
    private static XElement CreateXElement(LevelScalingEntry levelScalingEntry, StormStringValue stormStringValue, ReadOnlySpan<char> fullField, ReadOnlySpan<Range> fieldParts)
    {
        XElement xElement = new(levelScalingEntry.Catalog);
        XElement innerElement = xElement;

        foreach (Range fieldPart in fieldParts)
        {
            ReadOnlySpan<char> fieldPartSpan = fullField[fieldPart];

            innerElement = BuildInnerXElement(innerElement, fieldPartSpan);
        }

        innerElement.SetAttributeValue(ScaleAttributeName, stormStringValue.Value);

        return xElement;
    }

    private static XElement BuildInnerXElement(XElement xElement, ReadOnlySpan<char> fieldPartSpan)
    {
        if (fieldPartSpan[^1] == ']')
        {
            // PeriodicPeriodArray[0]
            int indexOfStartBracket = fieldPartSpan.LastIndexOf("[");

            // the value of the index, e.g. 0
            ReadOnlySpan<char> fieldPartIndexValue = fieldPartSpan.Slice(indexOfStartBracket + 1, fieldPartSpan.Length - indexOfStartBracket - 2);

            // the part with out the indexer, e.g. PeriodicPeriodArray
            ReadOnlySpan<char> fieldPartSpanWithoutIndexer = fieldPartSpan[..indexOfStartBracket];

            string fieldPart = fieldPartSpanWithoutIndexer.ToString();
            xElement.Add(new XElement(fieldPart, new XAttribute("index", fieldPartIndexValue.ToString())));

            return xElement.Element(fieldPart)!;
        }
        else
        {
            string fieldPart = fieldPartSpan.ToString();
            xElement.Add(new XElement(fieldPart));

            return xElement.Element(fieldPart)!;
        }
    }
}
