using Heroes.XmlData.GameStrings;
using System.ComponentModel.DataAnnotations;

namespace Heroes.XmlData.StormData;

internal static class ScaleValueParser
{
    public const string ScaleAttributeName = $"{HxdConstants.Name}Scale";

    public static StormElement? CreateStormElement(StormStorage stormStorage, LevelScalingEntry levelScalingEntry, StormStringValue stormStringValue)
    {
        //if (!stormStorage.TryGetExistingStormElementByDataObjectType(levelScalingEntry.Catalog, levelScalingEntry.Entry, out StormElement? originalStormElement))
        //    return null;


        //ReadOnlySpan<char> fieldSpan = levelScalingEntry.Field;
        //int splitterCount = fieldSpan.Count('.');

        //Span<Range> fieldParts = stackalloc Range[splitterCount + 1];

        //fieldSpan.Split(fieldParts, '.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        string? validatedField = GetValidatedField(stormStorage, levelScalingEntry);

        if (validatedField is not null)
        {
            ReadOnlySpan<char> fieldSpan = validatedField;
            int splitterCount = fieldSpan.Count('.');

            Span<Range> fieldParts = stackalloc Range[splitterCount + 1];

            fieldSpan.Split(fieldParts, '.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            XElement newScalingElement = CreateXElement(levelScalingEntry, stormStringValue, fieldSpan, fieldParts);
            StormElement stormScalingElement = new(new StormXElementValuePath(newScalingElement, stormStringValue.Path));

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



        //if (!stormStorage.TryGetExistingStormElementByDataObjectType(levelScalingEntry.Catalog, levelScalingEntry.Entry, out StormElement? stormElement))
        //   return;
        StormElement? completeStormElement = stormStorage.GetCompleteStormElement(levelScalingEntry.Catalog, levelScalingEntry.Entry);

        if (completeStormElement is null)
            return null;


        StormElementData lastData = DataRefParser.GetStormElementDataFromLastFieldPart(completeStormElement.DataValues, levelScalingEntry.Field, fieldParts);

        if (lastData.HasValue || lastData.HasConstValue)
        {
            //if (levelScalingEntry.Field.Equals(lastData.Field, StringComparison.OrdinalIgnoreCase))
            return lastData.Field;
        }

        return null;
    }

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

    //private void Test(StormElement stormElement, StormStringValue stormStringValue, ReadOnlySpan<char> fullField, ReadOnlySpan<Range> fieldParts)
    //{
    //    StormElementData stormElementData = stormElement.DataValues;

    //    XElement xElement = new(stormElement.ElementType);
    //    XElement innerElement = xElement;

    //    foreach (Range fieldPart in fieldParts)
    //    {
    //        ReadOnlySpan<char> fieldPartSpan = fullField[fieldPart];

    //        innerElement = BuildInnerXElement(innerElement, fieldPartSpan);
    //    }

    //    innerElement.SetAttributeValue(_scaleAttributeName, stormStringValue.Value);

    //    stormElement.AddValue(new StormXElementValuePath(xElement, stormStringValue.Path));
    //}



    //private StormElementData? ParseEntry(StormElement stormElement, LevelScalingEntry levelScalingEntry, ReadOnlySpan<Range> fieldParts)
    //{
    //    //StormElement? stormElement = _stormStorage.GetStormElementsByDataObjectType(levelScalingEntry.Catalog, levelScalingEntry.Entry);
    //   // if (stormElement is not null)
    //   // {
    //        return GetInnerElementData(stormElement, levelScalingEntry, fieldParts);
    //   // }

    //  //  return null;
    //}

    //private StormElementData? GetInnerElementData(StormElement stormElement, LevelScalingEntry levelScalingEntry, ReadOnlySpan<Range> fieldParts)
    //{
    //    StormElementData currentElementData = stormElement.DataValues;

    //    currentElementData = DataRefParser.GetStormElementDataFromLastFieldPart(currentElementData, levelScalingEntry.Field, fieldParts);

    //    if (currentElementData.HasConstValue || currentElementData.HasValue)
    //    {
    //        return currentElementData;
    //        //currentElementData.KeyValueDataPairs.Add(_scaleAttributeName, new StormElementData(stormStringValue.Value));

    //        //return true;
    //    }
    //    //else if ()
    //    //{
    //    //    //if (currentElementData.HasTextIndex)
    //    //    //    return GetValueScale(currentElementData.Value, fullPartSpan, xmlParts, currentElementData.KeyValueDataPairs.First().Key);
    //    //    //else
    //    //    //    return GetValueScale(currentElementData.Value, fullPartSpan, xmlParts);
    //    //}
    //    else if (stormElement.HasParentId)
    //    {
    //        // check the parents
    //        StormElement? parentStormElement = _stormStorage.GetStormElementsByDataObjectType(levelScalingEntry.Catalog, stormElement.ParentId);
    //        if (parentStormElement is not null)
    //            return ParseEntry(parentStormElement, new LevelScalingEntry(levelScalingEntry.Catalog, stormElement.ParentId, levelScalingEntry.Field), fieldParts);
    //    }
    //    //else if (currentElementType == ElementType.Normal)
    //    //{
    //    //    // then check the element type, which has no id attribute
    //    //    return ParseStormElementType(stormElement.ElementType, fullPartSpan, xmlParts);
    //    //}
    //    //else if (currentElementType == ElementType.Type)
    //    //{
    //    //    // then check the base element type, may not be the correct one, but close enough
    //    //    return ParseBaseElementType(stormElement.ElementType, fullPartSpan, xmlParts);
    //    //}

    //    return null;
    //}
}
