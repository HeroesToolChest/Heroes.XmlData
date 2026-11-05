using System.Runtime.InteropServices;

namespace Heroes.StormData.GameStrings;

internal sealed class DataRefAttributeParser
{
    private static readonly SearchValues<char> _gameStringOps = SearchValues.Create("+-*/()");
    private static readonly SearchValues<char> _catalogReferenceSplitters = SearchValues.Create(",.");

    private readonly LayeredDataStorage _layeredDataStorage;
    private readonly DataTagParser _dataTagParser;
    private readonly StormXmlElementMerger _stormXmlElementMerger;

    public DataRefAttributeParser(LayeredDataStorage layeredDataStorage, DataTagParser dataTagParser)
    {
        _layeredDataStorage = layeredDataStorage;
        _dataTagParser = dataTagParser;
        _stormXmlElementMerger = new StormXmlElementMerger(layeredDataStorage);
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
        var (size, sizeScaling) = GetSizeOfBuffer(dRefSpan, dataRefParts);

        double expressionValue = ComputeExpression(dRefSpan, size, dataRefParts, false);

        // there is scaling only if the sizeScaling is larger and the expression value is not 0, since a base value of 0 has no scaling ratio
        if (sizeScaling > size && expressionValue != 0)
        {
            double expressionWithScalingValue = ComputeExpression(dRefSpan, sizeScaling, dataRefParts, true);
            double scalingValue = (expressionWithScalingValue / expressionValue) - 1;

            if (scalingValue != 0 && double.IsFinite(scalingValue))
                return new ValueScale(expressionValue, scalingValue);
        }

        return new ValueScale(expressionValue);
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

    private static (int Size, int SizeScaling) GetSizeOfBuffer(ReadOnlySpan<char> dRefSpan, List<TextSectionItem> dataRefParts)
    {
        int size = 0;
        int scalingSize = 0;

        foreach (ref readonly TextSectionItem item in CollectionsMarshal.AsSpan(dataRefParts))
        {
            if (item.Type == TextSectionType.Text)
            {
                size += dRefSpan[item.Range].Length;
            }
            else if (item.Type == TextSectionType.Value)
            {
                size += item.ValueScale.TotalValueDigits() + 2; // +2 for ()

                // measure the multiplier, since that's what gets written, not the scaling itself
                if (item.ValueScale.Scaling.HasValue)
                    scalingSize += MathHelpers.GetCountOfDigits(GetScalingMultiplier(item.ValueScale.Scaling.Value)) + 1; // +1 for '*'
            }
        }

        return (size, size + scalingSize);
    }

    private static void FillExpressionBuffer(in TextSectionItem item, ReadOnlySpan<char> dRefSpan, Span<char> destination, bool includeScaling, ref int currentOffset)
    {
        if (item.Type == TextSectionType.Text)
        {
            ReadOnlySpan<char> itemText = dRefSpan[item.Range];

            itemText.CopyTo(destination[currentOffset..]);
            currentOffset += itemText.Length;
        }
        else if (item.Type == TextSectionType.Value)
        {
            ValueScale valueScale = item.ValueScale;

            // this value needs to be wrapped in parenthesis
            destination[currentOffset++] = '(';

            MathHelpers.WriteNumber(valueScale.Value, destination, ref currentOffset);

            if (includeScaling && valueScale.Scaling.HasValue)
            {
                destination[currentOffset++] = '*';

                MathHelpers.WriteNumber(GetScalingMultiplier(valueScale.Scaling.Value), destination, ref currentOffset);
            }

            destination[currentOffset++] = ')';
        }
    }

    // the value is multiplied by this to apply its scaling, e.g. a scaling of 0.04 is * 1.04
    private static double GetScalingMultiplier(double scaling) => scaling + 1;

    private static double ComputeExpression(ReadOnlySpan<char> dRefSpan, int bufferSize, List<TextSectionItem> dataRefParts, bool addScaling)
    {
        int currentOffset = 0;

        // buffer for the parsed expression
        Span<char> buffer = stackalloc char[bufferSize];

        foreach (ref readonly TextSectionItem item in CollectionsMarshal.AsSpan(dataRefParts))
        {
            FillExpressionBuffer(in item, dRefSpan, buffer, addScaling, ref currentOffset);
        }

        return StormCalculator.Compute(buffer[..currentOffset]);
    }

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

        StormXmlElement? mergedStormXmlElement = _stormXmlElementMerger.GetMerged(entry, catalog);
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
