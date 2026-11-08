using System.Runtime.InteropServices;

namespace Heroes.StormData.GameStrings;

internal sealed class DataRefAttributeParser
{
    private const int MaxStackAllocLength = 256;

    private static readonly SearchValues<char> _gameStringOps = SearchValues.Create("+-*/()");
    private static readonly SearchValues<char> _catalogReferenceSplitters = SearchValues.Create(",.");

    private readonly LayeredDataStorage _layeredDataStorage;
    private readonly DataTagParser _dataTagParser;

    public DataRefAttributeParser(LayeredDataStorage layeredDataStorage, DataTagParser dataTagParser)
    {
        _layeredDataStorage = layeredDataStorage;
        _dataTagParser = dataTagParser;
    }

    // Abil,GuldanHorrify,CastIntroTime+Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    public ValueScale Parse(ReadOnlySpan<char> dRefExpression)
    {
        List<TextSectionItem> catalogReferences = [];
        int startIndex = 0;

        while (startIndex < dRefExpression.Length)
        {
            // getting the part of the expression, e.g. Abil,GuldanHorrify,CastIntroTime
            ReadOnlySpan<char> catalogReference = GetCatalogReference(dRefExpression[startIndex..]);
            ReadOnlySpan<char> catalogReferenceTrimmed = catalogReference.Trim();

            if (!catalogReferenceTrimmed.IsEmpty)
            {
                if (catalogReferenceTrimmed[0] == '[')
                    catalogReferences.Add(new TextSectionItem(ParseBracketDRef(catalogReferenceTrimmed)));
                else if (double.TryParse(catalogReferenceTrimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    catalogReferences.Add(new TextSectionItem(new ValueScale(value))); // hardcoded value
                else if (catalogReferenceTrimmed[0] == '$' && catalogReferenceTrimmed[^1] == '$')
                    catalogReferences.Add(new TextSectionItem(new ValueScale(0)));
                else
                    catalogReferences.Add(new TextSectionItem(ParseCatalogReference(catalogReference)));
            }

            if (startIndex + catalogReference.Length >= dRefExpression.Length)
                break;

            // the operator
            catalogReferences.Add(new TextSectionItem(new Range(startIndex + catalogReference.Length, startIndex + catalogReference.Length + 1)));

            startIndex += catalogReference.Length + 1;
        }

        return CalculateExpression(dRefExpression, catalogReferences);
    }

    internal static ValueScale CalculateExpression(ReadOnlySpan<char> dRefSpan, List<TextSectionItem> dataRefParts)
    {
        ReadOnlySpan<TextSectionItem> parts = CollectionsMarshal.AsSpan(dataRefParts);

        int textLength = 0;
        int valueCount = 0;
        int scalingCount = 0;

        foreach (ref readonly TextSectionItem item in parts)
        {
            if (item.Type == TextSectionType.Text)
            {
                textLength += dRefSpan[item.Range].Length;
            }
            else if (item.Type == TextSectionType.Value)
            {
                valueCount++;

                if (item.ValueScale.Scaling.HasValue)
                    scalingCount++;
            }
        }

        // the buffers are sized for the expression with scaling, the larger of the two
        // a value is written as (#) or (#*#) with its scaling multiplier
        // # is the placeholder character
        int expressionLength = textLength + (valueCount * 3) + (scalingCount * 2);
        int valuesLength = valueCount + scalingCount;

        char[]? rentedExpression = null;
        double[]? rentedValues = null;

        Span<char> expressionBuffer = expressionLength <= MaxStackAllocLength
            ? stackalloc char[expressionLength]
            : (rentedExpression = ArrayPool<char>.Shared.Rent(expressionLength));

        Span<double> valuesBuffer = valuesLength <= MaxStackAllocLength
            ? stackalloc double[valuesLength]
            : (rentedValues = ArrayPool<double>.Shared.Rent(valuesLength));

        try
        {
            double expressionValue = ComputeExpression(dRefSpan, parts, expressionBuffer, valuesBuffer, false);

            // a base value of 0 has no scaling ratio
            if (scalingCount > 0 && expressionValue != 0)
            {
                double expressionWithScalingValue = ComputeExpression(dRefSpan, parts, expressionBuffer, valuesBuffer, true);
                double scalingValue = (expressionWithScalingValue / expressionValue) - 1;

                if (scalingValue != 0 && double.IsFinite(scalingValue))
                    return new ValueScale(expressionValue, scalingValue);
            }

            return new ValueScale(expressionValue);
        }
        finally
        {
            if (rentedExpression is not null)
                ArrayPool<char>.Shared.Return(rentedExpression);

            if (rentedValues is not null)
                ArrayPool<double>.Shared.Return(rentedValues);
        }
    }

    private static ReadOnlySpan<char> GetCatalogReference(ReadOnlySpan<char> text)
    {
        int indexOfOperator = text.IndexOfAny(_gameStringOps);

        // checking for inner bracket [d ref /]
        int indexOfStartBracket = text.IndexOf("[d ", StringComparison.OrdinalIgnoreCase);
        int indexOfEndBracket = text.IndexOf("/]") + 1;

        if (indexOfEndBracket > 0 && indexOfStartBracket < indexOfOperator && indexOfStartBracket > -1)
        {
            return text[indexOfStartBracket..(indexOfEndBracket + 1)];
        }

        if (indexOfOperator > -1)
            return text[..indexOfOperator];
        else
            return text;
    }

    // builds the expression with a placeholder for each number, and the numbers in the order they appear, so they don't have to be written as text and read back
    private static double ComputeExpression(ReadOnlySpan<char> dRefSpan, ReadOnlySpan<TextSectionItem> parts, Span<char> expressionBuffer, Span<double> valuesBuffer, bool includeScaling)
    {
        int expressionLength = 0;
        int valueCount = 0;

        foreach (ref readonly TextSectionItem item in parts)
        {
            if (item.Type == TextSectionType.Text)
            {
                ReadOnlySpan<char> itemText = dRefSpan[item.Range];

                itemText.CopyTo(expressionBuffer[expressionLength..]);
                expressionLength += itemText.Length;
            }
            else if (item.Type == TextSectionType.Value)
            {
                ValueScale valueScale = item.ValueScale;

                // this value needs to be wrapped in parenthesis.
                // the numbers are normalized the same way as when they were written as text
                expressionBuffer[expressionLength++] = '(';
                expressionBuffer[expressionLength++] = StormCalculator.ValuePlaceholder;
                valuesBuffer[valueCount++] = MathHelpers.Normalize(valueScale.Value);

                if (includeScaling && valueScale.Scaling.HasValue)
                {
                    expressionBuffer[expressionLength++] = '*';
                    expressionBuffer[expressionLength++] = StormCalculator.ValuePlaceholder;
                    valuesBuffer[valueCount++] = MathHelpers.Normalize(GetScalingMultiplier(valueScale.Scaling.Value));
                }

                expressionBuffer[expressionLength++] = ')';
            }
        }

        return StormCalculator.Compute(expressionBuffer[..expressionLength], valuesBuffer[..valueCount]);
    }

    // the value is multiplied by this to apply its scaling, e.g. a scaling of 0.04 is * 1.04
    private static double GetScalingMultiplier(double scaling) => scaling + 1;

    private ValueScale ParseBracketDRef(ReadOnlySpan<char> text)
    {
        Span<char> temp = stackalloc char[text.Length];
        text.CopyTo(temp);
        temp[0] = '<';
        temp[^1] = '>';

        return _dataTagParser.ParseDataTag(temp);
    }

    // Abil,GuldanHorrify,CastIntroTime
    // Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    // Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse
    private ValueScale ParseCatalogReference(ReadOnlySpan<char> catalogReference)
    {
        // a proper dref consists of <Catalog>,<Entry>,<Field>
        // <Field> is period separated
        int splitterCount = catalogReference.CountAny(',');

        if (splitterCount < 2)
            return new ValueScale(0);

        Span<Range> catalogReferencePartRanges = stackalloc Range[splitterCount + 1];

        catalogReference.SplitAny(catalogReferencePartRanges, ",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        ReadOnlySpan<char> catalog = catalogReference[catalogReferencePartRanges[0]];
        ReadOnlySpan<char> entry = catalogReference[catalogReferencePartRanges[1]];
        ReadOnlySpan<char> field = catalogReference[catalogReferencePartRanges[2]];

        StormXmlElement? mergedStormXmlElement = _layeredDataStorage.GetMergedStormXmlElementById(entry, catalog);
        if (mergedStormXmlElement is not null)
            return ParseField(mergedStormXmlElement, catalog, entry, field);

        return new ValueScale(0);
    }

    private ValueScale ParseField(StormXmlElement mergedStormXmlElement, ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field)
    {
        FieldNode fieldNode = DataRefFieldParser.ParseField(mergedStormXmlElement, field);

        if (fieldNode.IsEmpty || !fieldNode.Value.TryGetDouble(out double fieldValue))
            return new ValueScale(0);

        if (TryGetScaling(mergedStormXmlElement, catalog, entry, field, fieldNode, out double scaling))
            return new ValueScale(fieldValue, scaling);

        return new ValueScale(fieldValue);
    }

    // finds the LevelScalingArray modification whose Field resolves to the same node as the dref field
    private bool TryGetScaling(StormXmlElement mergedStormXmlElement, ReadOnlySpan<char> catalog, ReadOnlySpan<char> entry, ReadOnlySpan<char> field, FieldNode fieldNode, out double scaling)
    {
        scaling = 0;

        IReadOnlyList<LevelScalingFieldValue>? scalingFieldValues = _layeredDataStorage.GetScalingFieldValuesById(entry, catalog);
        if (scalingFieldValues is null)
            return false;

        // from the end, so if two spellings resolve to the same node, the later (higher data storage layer) one wins
        for (int i = scalingFieldValues.Count - 1; i >= 0; i--)
        {
            LevelScalingFieldValue scalingFieldValue = scalingFieldValues[i];

            // fast path: same text, so no need to parse it.
            // both are parsed on the same merged element, so the node instances can be compared
            bool isMatch = field.Equals(scalingFieldValue.Field, StringComparison.Ordinal) || DataRefFieldParser.ParseField(mergedStormXmlElement, scalingFieldValue.Field).IsSameNode(fieldNode);

            if (isMatch)
                return double.TryParse(scalingFieldValue.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out scaling);
        }

        return false;
    }
}
