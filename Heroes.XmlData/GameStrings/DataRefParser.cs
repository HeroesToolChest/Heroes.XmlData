using System.Buffers;

namespace Heroes.XmlData.GameStrings;

internal sealed class DataRefParser
{
    private static readonly SearchValues<char> _gameStringOps = SearchValues.Create("+-*/()");

    private readonly GameStringParser _gameStringParser;
    private readonly IStormStorage _stormStorage;

    public DataRefParser(GameStringParser gameStringParser, IStormStorage stormStorage)
    {
        _gameStringParser = gameStringParser;
        _stormStorage = stormStorage;
    }

    public static StormElementData GetStormElementDataFromLastFieldPart(StormElementData currentElementData, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> fieldParts)
    {
        foreach (Range fieldPartRange in fieldParts)
        {
            ReadOnlySpan<char> fieldPartSpan = fullPartSpan[fieldPartRange];

            if (fieldPartSpan.IsEmpty)
                continue;

            if (fieldPartSpan[^1] == ']')
            {
                // PeriodicPeriodArray[0]
                int indexOfStartBracket = fieldPartSpan.LastIndexOf("[");

                // the value of the index, e.g. 0
                ReadOnlySpan<char> fieldPartIndexValue = fieldPartSpan.Slice(indexOfStartBracket + 1, fieldPartSpan.Length - indexOfStartBracket - 2);
                bool numericalIndex = int.TryParse(fieldPartIndexValue, out int indexAsNumber);

                // the part with out the indexer, e.g. PeriodicPeriodArray
                ReadOnlySpan<char> fieldPartSpanWithoutIndexer = fieldPartSpan[..indexOfStartBracket];

                if (currentElementData.TryGetElementDataAt(fieldPartSpanWithoutIndexer, out StormElementData? withoutIndexerStormElementData) ||
                    currentElementData.ElementDataPairs.First().Value.TryGetElementDataAt(fieldPartSpanWithoutIndexer, out withoutIndexerStormElementData))
                {
                    if (withoutIndexerStormElementData.TryGetElementDataAt(fieldPartIndexValue, out StormElementData? indexStormElementData))
                    {
                        currentElementData = indexStormElementData;
                    }
                    else if (numericalIndex is true && withoutIndexerStormElementData.ElementDataCount > 0 && indexAsNumber < withoutIndexerStormElementData.ElementDataCount)
                    {
                        var indexValueElement = withoutIndexerStormElementData.ElementDataPairs.ElementAt(indexAsNumber);

                        // is this an indexed element?
                        if (indexValueElement.Value.IsIndexed)
                            currentElementData = indexValueElement.Value;
                        else
                            currentElementData = withoutIndexerStormElementData;
                    }
                    else if (withoutIndexerStormElementData.HasValue)
                    {
                        currentElementData = withoutIndexerStormElementData;
                    }
                    else if (withoutIndexerStormElementData.HasHxdScale)
                    {
                        currentElementData = withoutIndexerStormElementData;
                    }
                }
            }
            else if (currentElementData.TryGetElementDataAt(fieldPartSpan, out StormElementData? stormElementData))
            {
                if (stormElementData.ElementDataCount > 0)
                {
                    KeyValuePair<string, StormElementData> firstKeyValueElement = stormElementData.ElementDataPairs.First();

                    // accessed an element that is indexed without an indexer, grab the first
                    if (firstKeyValueElement.Value.IsIndexed)
                        currentElementData = firstKeyValueElement.Value;
                    else
                        currentElementData = stormElementData;
                }
                else
                {
                    currentElementData = stormElementData;
                }
            }
            else
            {
                return currentElementData;
            }
        }

        return currentElementData;
    }

    // Abil,GuldanHorrify,CastIntroTime+Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    public ValueScale Parse(ReadOnlySpan<char> dRefSpan)
    {
        List<ITextSection> dataRefParts = [];
        int startIndex = 0;

        while (startIndex < dRefSpan.Length)
        {
            // getting the part of the expression, e.g. Abil,GuldanHorrify,CastIntroTime
            ReadOnlySpan<char> partSpan = GetNextPart(dRefSpan[startIndex..]);
            ReadOnlySpan<char> partSpanTrimmed = partSpan.Trim();

            if (!partSpanTrimmed.IsEmpty)
            {
                if (partSpanTrimmed[0] == '[')
                    dataRefParts.Add(new TextSectionValueScale(ParseBracketDRef(partSpanTrimmed)));
                else if (double.TryParse(partSpanTrimmed, out double value))
                    dataRefParts.Add(new TextSectionValueScale(new ValueScale(value))); // hardcoded value
                else if (partSpanTrimmed[0] == '$' && partSpanTrimmed[^1] == '$')
                    dataRefParts.Add(new TextSectionValueScale(new ValueScale(0)));
                else
                    dataRefParts.Add(new TextSectionValueScale(ParsePart(partSpan)));
            }

            if (startIndex + partSpan.Length >= dRefSpan.Length)
                break;

            // the operator
            dataRefParts.Add(new TextSection(new Range(startIndex + partSpan.Length, startIndex + partSpan.Length + 1)));

            startIndex += partSpan.Length + 1;
        }

        return CalculateExpression(dRefSpan, dataRefParts);
    }

    private static (int Size, int SizeScaling) GetSizeOfBuffer(ReadOnlySpan<char> dRefSpan, List<ITextSection> dataRefParts)
    {
        int size = 0;
        int scalingSize = 0;

        foreach (ITextSection dataSection in dataRefParts)
        {
            if (dataSection.Type == TextSectionType.Text)
            {
                size += dRefSpan[((TextSection)dataSection).Range].Length;
            }
            else if (dataSection.Type == TextSectionType.Value)
            {
                TextSectionValueScale textSectionValueScale = (TextSectionValueScale)dataSection;

                size += textSectionValueScale.ValueScale.TotalValueDigits() + 2; // +2 for ()

                int scalingDigits = textSectionValueScale.ValueScale.TotalScalingDigits();
                if (scalingDigits > 0)
                    scalingSize += scalingDigits + 1; // +1 for '*';
            }
        }

        return (size, size + scalingSize);
    }

    private static ReadOnlySpan<char> GetNextPart(ReadOnlySpan<char> text)
    {
        int indexOfOperator = text.IndexOfAny(_gameStringOps);

        // checking for inner bracket [d ref /]
        int indexOfStartBracket = text.IndexOf("[d", StringComparison.OrdinalIgnoreCase);
        if (indexOfStartBracket < indexOfOperator && indexOfStartBracket > -1)
        {
            int indexOfEndBracket = text.IndexOf("/]") + 1;

            return text[indexOfStartBracket..(indexOfEndBracket + 1)];
        }

        if (indexOfOperator > -1)
            return text[..indexOfOperator];
        else
            return text;
    }

    private static void FillExpressionBuffer(ITextSection dataSection, ReadOnlySpan<char> dRefSpan, Span<char> destination, bool includeScaling, ref int currentOffset)
    {
        if (dataSection.Type == TextSectionType.Text)
        {
            TextSection textSection = (TextSection)dataSection;

            ReadOnlySpan<char> itemText = dRefSpan[textSection.Range];

            itemText.CopyTo(destination[currentOffset..]);
            currentOffset += itemText.Length;
        }
        else if (dataSection.Type == TextSectionType.Value)
        {
            TextSectionValueScale textSectionValueScale = (TextSectionValueScale)dataSection;

            destination[currentOffset..][0] = '(';
            currentOffset++;

            // this value needs to be wrapped in parenthesis
            textSectionValueScale.ValueScale.Value.TryFormat(destination[currentOffset..], out int charsWritten);
            currentOffset += charsWritten;

            if (includeScaling && textSectionValueScale.ValueScale.Scaling.HasValue)
            {
                destination[currentOffset..][0] = '*';
                currentOffset++;

                (textSectionValueScale.ValueScale.Scaling.Value + 1).TryFormat(destination[currentOffset..], out charsWritten);
                currentOffset += charsWritten;
            }

            destination[currentOffset..][0] = ')';
            currentOffset++;
        }
    }

    private ValueScale ParseBracketDRef(ReadOnlySpan<char> text)
    {
        Span<char> temp = stackalloc char[text.Length];
        text.CopyTo(temp);
        temp[0] = '<';
        temp[^1] = '>';

        return _gameStringParser.ParseDataTag(temp);
    }

    // Abil,GuldanHorrify,CastIntroTime
    // Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    // Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse
    private ValueScale ParsePart(ReadOnlySpan<char> fullPartSpan)
    {
        // a proper dref consists of <Catalog>.<Entry>.<Field>
        int splitterCount = fullPartSpan.Count('.');

        // <Field>s are comma separated
        splitterCount += fullPartSpan.Count(',');

        if (splitterCount < 2)
            return new ValueScale(0);

        Span<Range> xmlParts = stackalloc Range[splitterCount + 1];

        fullPartSpan.SplitAny(xmlParts, ".,", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return ParseStormElement(fullPartSpan, fullPartSpan[xmlParts[1]], xmlParts);
    }

    private ValueScale ParseStormElement(ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<char> entry, ReadOnlySpan<Range> xmlParts)
    {
#if NET9_0_OR_GREATER
        StormElement? stormElement = _stormStorage.GetCompleteStormElement(entry, fullPartSpan[xmlParts[0]]);
#else
        StormElement? stormElement = _stormStorage.GetCompleteStormElement(entry.ToString(), fullPartSpan[xmlParts[0]].ToString());
#endif

        return ParseFields(stormElement, fullPartSpan, xmlParts);
    }

    private ValueScale ParseFields(StormElement? stormElement, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> xmlParts)
    {
        if (stormElement is not null)
        {
            return ParseFieldParts(stormElement, fullPartSpan, xmlParts);
        }

        return new ValueScale(0);
    }

    private ValueScale ParseFieldParts(StormElement stormElement, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> xmlParts)
    {
        StormElementData currentElementData = stormElement.DataValues;

        ReadOnlySpan<Range> fieldParts = xmlParts[2..];

        currentElementData = GetStormElementDataFromLastFieldPart(currentElementData, fullPartSpan, fieldParts);

        if (currentElementData.HasValue)
        {
            return GetValueScale(currentElementData.RawValue, fullPartSpan, xmlParts);
        }

        return new ValueScale(0);
    }

    private ValueScale GetValueScale(ReadOnlySpan<char> stormElementDataValue, ReadOnlySpan<char> fullSpan, ReadOnlySpan<Range> xmlParts, ReadOnlySpan<char> fieldIndexer = default)
    {
        if (!double.TryParse(stormElementDataValue, out double dataValue))
            return new ValueScale(0);

#if NET9_0_OR_GREATER
        StormElement? scalingStormElement = _stormStorage.GetScaleValueStormElementById(fullSpan[xmlParts[1]], fullSpan[xmlParts[0]]);
#else
        StormElement? scalingStormElement = _stormStorage.GetScaleValueStormElementById(fullSpan[xmlParts[1]].ToString(), fullSpan[xmlParts[0]].ToString());
#endif
        if (scalingStormElement is null)
            return new ValueScale(dataValue);

        StormElementData stormElementData = GetStormElementDataFromLastFieldPart(scalingStormElement.DataValues, fullSpan, xmlParts[2..]);

        // AmountArray[Quest] or FlatModifierArray[0].Modifier where Modifier is in [0]
        if ((!fieldIndexer.IsEmpty && stormElementData.TryGetElementDataAt(fieldIndexer, out StormElementData? innerIndexData)) ||
            (fieldIndexer.IsEmpty && stormElementData.ElementDataPairs.Count == 1 && stormElementData.TryGetElementDataAt("0", out innerIndexData)))
        {
            stormElementData = innerIndexData;
        }

        if (stormElementData.HasHxdScale && stormElementData.HxdScaleValue.TryGetDouble(out double scalingValue))
            return new ValueScale(dataValue, scalingValue);
        else
            return new ValueScale(dataValue);
    }

    private ValueScale CalculateExpression(ReadOnlySpan<char> dRefSpan, List<ITextSection> dataRefParts)
    {
        var (size, sizeScaling) = GetSizeOfBuffer(dRefSpan, dataRefParts);

        double expressionValue = ComputeExpression(dRefSpan, size, dataRefParts, false);

        double? expressionWithScalingValue = null;

        // there is scaling only if the sizeScaling is larger
        if (sizeScaling > size)
        {
            expressionWithScalingValue = ComputeExpression(dRefSpan, sizeScaling, dataRefParts, true);
        }

        if (expressionWithScalingValue.HasValue)
        {
            double scalingValue = (expressionWithScalingValue.Value / expressionValue) - 1;

            if (scalingValue == 0)
                return new ValueScale(expressionValue);
            else
                return new ValueScale(expressionValue, scalingValue);
        }

        return new ValueScale(expressionValue);
    }

    private double ComputeExpression(ReadOnlySpan<char> dRefSpan, int bufferSize, List<ITextSection> dataRefParts, bool addScaling)
    {
        int currentOffset = 0;

        // buffer for the parsed expression
        Span<char> buffer = stackalloc char[bufferSize];

        foreach (ITextSection dataSection in dataRefParts)
        {
            FillExpressionBuffer(dataSection, dRefSpan, buffer, addScaling, ref currentOffset);
        }

        return HeroesCalculator.Compute(buffer[..currentOffset]);
    }
}
