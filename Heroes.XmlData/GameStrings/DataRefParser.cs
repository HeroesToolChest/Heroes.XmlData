using Heroes.XmlData.StormMath;
using System.Buffers;

namespace Heroes.XmlData.GameStrings;

internal class DataRefParser
{
    private static readonly SearchValues<char> _gameStringOps = SearchValues.Create("+-*/()");

    private readonly GameStringParser _gameStringParser;
    private readonly HeroesData _heroesData;

    public DataRefParser(GameStringParser gameStringParser, HeroesData heroesData)
    {
        _gameStringParser = gameStringParser;
        _heroesData = heroesData;
    }

    // Abil,GuldanHorrify,CastIntroTime+Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    public ValueScale Parse(ReadOnlySpan<char> buffer)
    {
        List<ITextSection> dataRefParts = [];
        int startIndex = 0;

        while (startIndex < buffer.Length)
        {
            // getting the part of the expression, e.g. Abil,GuldanHorrify,CastIntroTime
            ReadOnlySpan<char> partSpan = GetNextPart(buffer[startIndex..]);

            if (!partSpan.IsEmpty && !partSpan.IsWhiteSpace())
            {
                if (partSpan[0] == '[')
                    dataRefParts.Add(new TextSectionValueScale(ParseBracketDRef(partSpan)));
                else if (double.TryParse(partSpan.Trim(), out double value))
                    dataRefParts.Add(new TextSectionValueScale(new ValueScale(value))); // hardcoded value
                else
                    dataRefParts.Add(new TextSectionValueScale(ParsePart(partSpan)));
            }

            if (startIndex + partSpan.Length >= buffer.Length)
                break;

            // the operator
            dataRefParts.Add(new TextSection(new Range(startIndex + partSpan.Length, startIndex + partSpan.Length + 1)));

            startIndex += partSpan.Length + 1;
        }

        return CalculateExpression(buffer, dataRefParts);
    }

    private static (int Size, int SizeScaling) GetSizeOfBuffer(ReadOnlySpan<char> buffer, List<ITextSection> dataRefParts)
    {
        int size = 0;
        int sizeWithScaling = 0;

        foreach (ITextSection dataSection in dataRefParts)
        {
            if (dataSection.Type == TextSectionType.Text)
            {
                size += buffer[((TextSection)dataSection).Range].Length;
            }
            else if (dataSection.Type == TextSectionType.Value)
            {
                size += GameStringParser.MaxNumberLength + 2; // +2 for ()
                if (((TextSectionValueScale)dataSection).ValueScale.Scaling.HasValue)
                    sizeWithScaling += GameStringParser.MaxScalingLength;
            }
        }

        return (size, size + sizeWithScaling);
    }

    private static ReadOnlySpan<char> GetNextPart(ReadOnlySpan<char> text)
    {
        int indexOfOperator = text.IndexOfAny(_gameStringOps);

        // checking for inner bracket [d ref /]
        int indexOfStartBracket = text.IndexOf("[d");
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

    private static void FillExpressionBuffer(ITextSection dataSection, ReadOnlySpan<char> buffer, Span<char> newBufferSpan, bool includeScaling, ref int currentOffset)
    {
        if (dataSection.Type == TextSectionType.Text)
        {
            TextSection textSection = (TextSection)dataSection;

            ReadOnlySpan<char> itemText = buffer[textSection.Range];

            itemText.CopyTo(newBufferSpan[currentOffset..]);
            currentOffset += itemText.Length;
        }
        else if (dataSection.Type == TextSectionType.Value)
        {
            TextSectionValueScale textSectionValueScale = (TextSectionValueScale)dataSection;

            newBufferSpan[currentOffset..][0] = '(';
            currentOffset++;

            // this value needs to be wrapped in parenthesis
            textSectionValueScale.ValueScale.Value.TryFormat(newBufferSpan[currentOffset..], out int charsWritten);
            currentOffset += charsWritten;

            if (includeScaling && textSectionValueScale.ValueScale.Scaling.HasValue)
            {
                newBufferSpan[currentOffset..][0] = '*';
                currentOffset++;

                (textSectionValueScale.ValueScale.Scaling.Value + 1).TryFormat(newBufferSpan[currentOffset..], out charsWritten);
                currentOffset += charsWritten;
            }

            newBufferSpan[currentOffset..][0] = ')';
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
        int splitterCount = fullPartSpan.Count('.');
        splitterCount += fullPartSpan.Count(',');

        if (splitterCount < 2)
            throw new Exception("TODO");

        Span<Range> xmlParts = stackalloc Range[splitterCount + 1];

        fullPartSpan.SplitAny(xmlParts, ".,", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (_heroesData.TryGetStormElement(fullPartSpan[xmlParts[0]], fullPartSpan[xmlParts[1]], out StormElement? stormElement))
        {
            return ParseParts(stormElement.DataValues, fullPartSpan, xmlParts);
        }

        return new ValueScale(0);
    }

    private ValueScale ParseParts(StormElementData stormElementData, ReadOnlySpan<char> fullPartSpan, Span<Range> xmlParts)
    {
        for (int i = 2; i < xmlParts.Length; i++)
        {
            ReadOnlySpan<char> nextPartSpan = fullPartSpan[xmlParts[i]];

            if (nextPartSpan[^1] == ']')
            {
                // PeriodicPeriodArray[0]
                int indexOfStartBracket = nextPartSpan.LastIndexOf("[");

                // the value of the index, e.g. 0
                ReadOnlySpan<char> indexValueSpan = nextPartSpan.Slice(indexOfStartBracket + 1, nextPartSpan.Length - indexOfStartBracket - 2);

                // the part with out the indexer, e.g. PeriodicPeriodArray
                ReadOnlySpan<char> nextPartWithoutIndexerSpan = nextPartSpan[..indexOfStartBracket];

                if (stormElementData.KeyValueDataPairs.TryGetValue(nextPartWithoutIndexerSpan.ToString(), out StormElementData? innerData))
                {
                    if (innerData.KeyValueDataPairs.TryGetValue(indexValueSpan.ToString(), out StormElementData? arrayData))
                        stormElementData = arrayData;
                }
            }
            else if (stormElementData.KeyValueDataPairs.TryGetValue(nextPartSpan.ToString(), out StormElementData? innerData))
            {
                stormElementData = innerData;
            }
        }

        if (stormElementData.HasConstValue)
        {
            return GetValueScale(stormElementData.ConstValue, fullPartSpan, xmlParts);
        }
        else if (stormElementData.IsArray)
        {
            KeyValuePair<string, StormElementData> firstKeyValueDataPair = stormElementData.KeyValueDataPairs.FirstOrDefault();

            return GetValueScale(firstKeyValueDataPair.Value.Value, fullPartSpan, xmlParts, firstKeyValueDataPair.Key);
        }
        else
        {
            return GetValueScale(stormElementData.Value, fullPartSpan, xmlParts);
        }
    }

    private ValueScale GetValueScale(ReadOnlySpan<char> stormElementDataValue, ReadOnlySpan<char> fullSpan, Span<Range> xmlParts, ReadOnlySpan<char> fieldIndexer = default)
    {
        if (double.TryParse(stormElementDataValue, out double dataValue))
        {
            ReadOnlySpan<char> fieldSpan = fullSpan[xmlParts[2]];

            if (!fieldIndexer.IsEmpty)
            {
                Span<char> buffer = stackalloc char[fieldSpan.Length + fieldIndexer.Length + 2]; // 2 for []
                fieldSpan.CopyTo(buffer);

                buffer[fieldSpan.Length..][0] = '[';
                fieldIndexer.CopyTo(buffer[(fieldSpan.Length + 1)..]);
                buffer[^1] = ']';

                return ScalingLookUp(fullSpan, xmlParts, dataValue, buffer);
            }

            return ScalingLookUp(fullSpan, xmlParts, dataValue, fieldSpan);
        }

        return new ValueScale(0);

        ValueScale ScalingLookUp(ReadOnlySpan<char> fullSpan, Span<Range> xmlParts, double dataValue, ReadOnlySpan<char> fieldSpan)
        {
            if (_heroesData.TryGetLevelScalingValue(fullSpan[xmlParts[0]], fullSpan[xmlParts[1]], fieldSpan, out StormStringValue? stormStringValue))
            {
                if (double.TryParse(stormStringValue.Value, out double scalingValue))
                    return new ValueScale(dataValue, scalingValue);
                else
                    return new ValueScale(0);
            }
            else
            {
                return new ValueScale(dataValue);
            }
        }
    }

    private ValueScale CalculateExpression(ReadOnlySpan<char> buffer, List<ITextSection> dataRefParts)
    {
        var (size, sizeScaling) = GetSizeOfBuffer(buffer, dataRefParts);

        int currentOffset = 0;
        double bufferValue;
        double? scaleBufferValue = null;

        // buffer for the parsed expression
        Span<char> newBufferSpan = stackalloc char[size];

        foreach (ITextSection dataSection in dataRefParts)
        {
            FillExpressionBuffer(dataSection, buffer, newBufferSpan, false, ref currentOffset);
        }

        bufferValue = HeroesCalculator.Compute(newBufferSpan[..currentOffset]);

        // there is scaling only if the sizeScaling is larger
        if (sizeScaling > size)
        {
            int currentScaleOffset = 0;

            // buffer for scaling
            Span<char> newBufferScalingSpan = stackalloc char[sizeScaling];

            foreach (ITextSection dataSection in dataRefParts)
            {
                FillExpressionBuffer(dataSection, buffer, newBufferScalingSpan, true, ref currentScaleOffset);
            }

            scaleBufferValue = HeroesCalculator.Compute(newBufferScalingSpan[..currentScaleOffset]);
        }

        if (scaleBufferValue.HasValue)
        {
            double scalingValue = (scaleBufferValue.Value / bufferValue) - 1;

            if (scalingValue == 0)
                return new ValueScale(bufferValue);
            else
                return new ValueScale(bufferValue, scalingValue);
        }

        return new ValueScale(bufferValue);
    }
}
