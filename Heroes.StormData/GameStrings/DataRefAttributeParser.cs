namespace Heroes.StormData.GameStrings;

internal sealed class DataRefAttributeParser
{
    // for calculate expressions
    private const int MaxStackAllocLength = 256;

    // up to this many parts are kept on the stack (a TextSectionItem is about 48 bytes), more are rented from the array pool
    private const int MaxStackAllocPartCount = 32;

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
        // each operator is a part, with at most one value part before it and one after.
        // this can be more than needed but never less
        int maxPartCount = (dRefExpression.CountAny(_gameStringOps) * 2) + 1;

        TextSectionItem[]? rentedParts = null;

        Span<TextSectionItem> dataRefParts = maxPartCount <= MaxStackAllocPartCount
            ? stackalloc TextSectionItem[maxPartCount]
            : (rentedParts = ArrayPool<TextSectionItem>.Shared.Rent(maxPartCount));

        int partCount = 0;

        try
        {
            int startIndex = 0;

            while (startIndex < dRefExpression.Length)
            {
                // getting the part of the expression, e.g. Abil,GuldanHorrify,CastIntroTime
                ReadOnlySpan<char> catalogReference = GetCatalogReference(dRefExpression[startIndex..], out int catalogReferenceLength);
                ReadOnlySpan<char> catalogReferenceTrimmed = catalogReference.Trim();

                if (!catalogReferenceTrimmed.IsEmpty)
                {
                    if (catalogReferenceTrimmed[0] == '[')
                        dataRefParts[partCount++] = new TextSectionItem(_dataTagParser.ParseBracketDataTag(catalogReferenceTrimmed));
                    else if (double.TryParse(catalogReferenceTrimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        dataRefParts[partCount++] = new TextSectionItem(new ValueScale(value)); // hardcoded value
                    else if (catalogReferenceTrimmed[0] == '$' && catalogReferenceTrimmed[^1] == '$')
                        dataRefParts[partCount++] = new TextSectionItem(new ValueScale(0));
                    else
                        dataRefParts[partCount++] = new TextSectionItem(ParseCatalogReference(catalogReference));
                }

                int indexOfOperator = startIndex + catalogReferenceLength;

                if (indexOfOperator >= dRefExpression.Length)
                    break;

                // the operator
                dataRefParts[partCount++] = new TextSectionItem(new Range(indexOfOperator, indexOfOperator + 1));

                startIndex = indexOfOperator + 1;
            }

            return CalculateExpression(dRefExpression, dataRefParts[..partCount]);
        }
        finally
        {
            if (rentedParts is not null)
                ArrayPool<TextSectionItem>.Shared.Return(rentedParts);
        }
    }

    internal static ValueScale CalculateExpression(ReadOnlySpan<char> dRefSpan, ReadOnlySpan<TextSectionItem> parts)
    {
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

    // returns the part of the expression before the next operator, and in length how many characters of text it takes up.
    // they differ for an inner bracket with text before it, such as spaces, since only the bracket is returned
    private static ReadOnlySpan<char> GetCatalogReference(ReadOnlySpan<char> text, out int length)
    {
        int indexOfOperator = text.IndexOfAny(_gameStringOps);

        // no operator, so there is no inner bracket either, since its /] has one
        if (indexOfOperator < 0)
        {
            length = text.Length;
            return text;
        }

        // checking for inner bracket [d ref /], which only counts if it starts before the operator, so only that part is searched.
        // "[d " has no operator characters, so a match that starts before the operator also ends before it
        int indexOfStartBracket = text[..indexOfOperator].IndexOf("[d ", StringComparison.OrdinalIgnoreCase);

        if (indexOfStartBracket > -1)
        {
            // the / is an operator, so the first /] can't be before the operator, which is after the start bracket
            int indexOfEndBracket = text[indexOfStartBracket..].IndexOf("/]");

            if (indexOfEndBracket > -1)
            {
                // up to and including the ]
                length = indexOfStartBracket + indexOfEndBracket + 2;
                return text[indexOfStartBracket..length];
            }
        }

        length = indexOfOperator;
        return text[..indexOfOperator];
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
