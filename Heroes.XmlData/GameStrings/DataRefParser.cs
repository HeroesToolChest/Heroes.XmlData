﻿using Heroes.XmlData.StormMath;
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

                if (currentElementData.KeyValueDataPairs.TryGetValue(fieldPartSpanWithoutIndexer.ToString(), out StormElementData? withoutIndexerStormElementData))
                {
                    if (((numericalIndex is true && withoutIndexerStormElementData.HasNumericalIndex is true) ||
                        (numericalIndex is false && withoutIndexerStormElementData.HasTextIndex is true)) && withoutIndexerStormElementData.KeyValueDataPairs.TryGetValue(fieldPartIndexValue.ToString(), out StormElementData? indexStormElementData))
                    {
                        currentElementData = indexStormElementData;
                    }
                    else if (numericalIndex is true && withoutIndexerStormElementData.HasTextIndex is true &&
                        withoutIndexerStormElementData.KeyValueDataPairs.Keys.Count > 0 && indexAsNumber < withoutIndexerStormElementData.KeyValueDataPairs.Keys.Count)
                    {
                        currentElementData = withoutIndexerStormElementData.KeyValueDataPairs.ElementAt(indexAsNumber).Value;
                    }
                    else if (numericalIndex is true) //withoutIndexerStormElementData.KeyValueDataPairs.Count > 1)
                    {
                        if (indexAsNumber == 0)
                        {
                            currentElementData = withoutIndexerStormElementData;
                        }
                        else
                        {
                            return currentElementData;
                        }
                    }
                    else if (withoutIndexerStormElementData.HasValue || withoutIndexerStormElementData.HasConstValue)
                    {
                        currentElementData = withoutIndexerStormElementData;
                    }
                    else if (withoutIndexerStormElementData.HasHxdScale)
                    {
                        currentElementData = withoutIndexerStormElementData;
                    }
                }
            }
            else if (currentElementData.KeyValueDataPairs.TryGetValue(fieldPartSpan.ToString(), out StormElementData? stormElementData) ||
                ((currentElementData.HasNumericalIndex || currentElementData.HasTextIndex) && currentElementData.KeyValueDataPairs.First().Value.KeyValueDataPairs.TryGetValue(fieldPartSpan.ToString(), out stormElementData)))
            {
                currentElementData = stormElementData;
            }
            else
            {
                return currentElementData;
            }
        }

        return currentElementData;
    }

    private static (int Size, int SizeScaling) GetSizeOfBuffer(ReadOnlySpan<char> dRefSpan, List<ITextSection> dataRefParts)
    {
        int size = 0;
        int sizeWithScaling = 0;

        foreach (ITextSection dataSection in dataRefParts)
        {
            if (dataSection.Type == TextSectionType.Text)
            {
                size += dRefSpan[((TextSection)dataSection).Range].Length;
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
        StormElement? stormElement = _heroesData.GetCompleteStormElement(fullPartSpan[xmlParts[0]].ToString(), entry.ToString());
        //StormElement? stormElement = _heroesData.GetStormElement(fullPartSpan[xmlParts[0]], entry);

        return ParseFields(stormElement, fullPartSpan, xmlParts); //, ElementType.Normal);
    }

    //private ValueScale ParseStormElementType(ReadOnlySpan<char> elementType, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> xmlParts)
    //{
    //    StormElement? stormElement = _heroesData.GetStormElement(elementType);

    //    return ParseFields(stormElement, fullPartSpan, xmlParts, ElementType.Type);
    //}

    //private ValueScale ParseBaseElementType(ReadOnlySpan<char> elementType, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> xmlParts)
    //{
    //    StormElement? stormElement = _heroesData.GetBaseStormElement(elementType);

    //    return ParseFields(stormElement, fullPartSpan, xmlParts, ElementType.Base);
    //}

    private ValueScale ParseFields(StormElement? stormElement, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> xmlParts) //, ElementType parseAs)
    {
        if (stormElement is not null)
        {
            return ParseFieldParts(stormElement, fullPartSpan, xmlParts);
        }

        return new ValueScale(0);
    }

    private ValueScale ParseFieldParts(StormElement stormElement, ReadOnlySpan<char> fullPartSpan, ReadOnlySpan<Range> xmlParts) //, ElementType currentElementType)
    {
        StormElementData currentElementData = stormElement.DataValues;

        ReadOnlySpan<Range> fieldParts = xmlParts[2..];

        currentElementData = GetStormElementDataFromLastFieldPart(currentElementData, fullPartSpan, fieldParts);

        if (currentElementData.HasConstValue)
        {
            return GetValueScale(currentElementData.ConstValue, fullPartSpan, xmlParts);
        }
        else if (currentElementData.HasValue)
        {
            if (currentElementData.HasTextIndex)
                return GetValueScale(currentElementData.Value, fullPartSpan, xmlParts, currentElementData.KeyValueDataPairs.First().Key);
            else
                return GetValueScale(currentElementData.Value, fullPartSpan, xmlParts);
        }
        //else if (stormElement.HasParentId)
        //{
        //    // check the parents
        //    return ParseStormElement(fullPartSpan, stormElement.ParentId, xmlParts);
        //}
        //else if (currentElementType == ElementType.Normal)
        //{
        //    // then check the element type, which has no id attribute
        //    return ParseStormElementType(stormElement.ElementType, fullPartSpan, xmlParts);
        //}
        //else if (currentElementType == ElementType.Type)
        //{
        //    // then check the base element type, may not be the correct one, but close enough
        //    return ParseBaseElementType(stormElement.ElementType, fullPartSpan, xmlParts);
        //}

        return new ValueScale(0);
    }

    private ValueScale GetValueScale(ReadOnlySpan<char> stormElementDataValue, ReadOnlySpan<char> fullSpan, ReadOnlySpan<Range> xmlParts, ReadOnlySpan<char> fieldIndexer = default)
    {
        if (double.TryParse(stormElementDataValue, out double dataValue))
        {
            StormElement? scalingStormElement = _heroesData.GetScalingStormElement(fullSpan[xmlParts[0]], fullSpan[xmlParts[1]]);
            if (scalingStormElement is null)
                return new ValueScale(dataValue);

            StormElementData stormElementData = GetStormElementDataFromLastFieldPart(scalingStormElement.DataValues, fullSpan, xmlParts[2..]);

            // AmountArray[Quest]
            if (!fieldIndexer.IsEmpty && stormElementData.KeyValueDataPairs.TryGetValue(fieldIndexer.ToString(), out StormElementData? innerIndexData))
            {
                stormElementData = innerIndexData;
            }

            if (stormElementData.HasHxdScale && double.TryParse(stormElementData.ScaleValue, out double scalingValue))
                return new ValueScale(dataValue, scalingValue);
            else
                return new ValueScale(dataValue);
        }

        return new ValueScale(0);
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
