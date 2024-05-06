using Heroes.XmlData.StormMath;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Heroes.XmlData.GameStrings;

internal static class DataRefParser
{
    private static readonly SearchValues<char> _gameStringOps = SearchValues.Create("+-*/()");

    public static ValueScale Parse(ReadOnlySpan<char> buffer, HeroesData heroesData)
    {
        List<TextSection> dataRefParts = [];
        // Abil,GuldanHorrify,CastIntroTime+Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]

        int startIndex = 0;
        while (startIndex < buffer.Length)
        {
            // getting the part of the expression
            ReadOnlySpan<char> partSpan = GetNextPart(buffer[startIndex..]);

            if (partSpan.IsEmpty)
                continue;

            if (double.TryParse(partSpan.Trim(), out double value))
                dataRefParts.Add(new TextSection(new ValueScale(value))); // hardcoded value
            else
                dataRefParts.Add(new TextSection(ParsePart(partSpan, heroesData)));

            // the operator
            if (startIndex + partSpan.Length >= buffer.Length)
                break;

            dataRefParts.Add(new TextSection(new Range(startIndex + partSpan.Length, startIndex + partSpan.Length + 1)));

            startIndex += partSpan.Length + 1;
        }

        return CalculateExpression(buffer, dataRefParts);
    }

    private static ReadOnlySpan<char> GetNextPart(ReadOnlySpan<char> text)
    {
        int indexOfOperator = text.IndexOfAny(_gameStringOps);
        if (indexOfOperator > -1)
            return text[..indexOfOperator];
        else
            return text;
    }

    // Abil,GuldanHorrify,CastIntroTime
    // Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    // Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse
    private static ValueScale ParsePart(ReadOnlySpan<char> partSpan, HeroesData heroesData)
    {
        int splitterCount = partSpan.Count('.');
        splitterCount += partSpan.Count(',');

        if (splitterCount < 2)
            throw new Exception("TODO");

        Span<Range> xmlParts = stackalloc Range[splitterCount + 1];

        partSpan.SplitAny(xmlParts, ".,", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (heroesData.TryGetStormElement(partSpan[xmlParts[0]], partSpan[xmlParts[1]], out StormElement? stormElement))
        {
            ReadOnlySpan<char> thirdPartSpan = partSpan[xmlParts[2]];

            // check for indexer
            if (thirdPartSpan.EndsWith("]"))
            {
                int indexOfStartBracket = thirdPartSpan.LastIndexOf("[");

                // the value of the index
                ReadOnlySpan<char> indexValueSpan = thirdPartSpan.Slice(indexOfStartBracket + 1, thirdPartSpan.Length - indexOfStartBracket - 2);

                // the third part name (without the indexer)
                ReadOnlySpan<char> thirdPartValueSpan = thirdPartSpan.Slice(0, indexOfStartBracket);

                // find the value
                if (stormElement.DataValues is not null && stormElement.DataValues.KeyValueDataPairs.TryGetValue(thirdPartValueSpan.ToString(), out StormElementData? stormElementData))
                {
                    if (stormElementData.KeyValueDataPairs.TryGetValue(indexValueSpan.ToString(), out StormElementData? stormElementDataInner2))
                    {
                        return GetValueScale(stormElementDataInner2.Value, partSpan, heroesData, xmlParts);
                    }
                }
            }
            else
            {
                if (stormElement.DataValues is not null && stormElement.DataValues.KeyValueDataPairs.TryGetValue(thirdPartSpan.ToString(), out StormElementData? stormElementData))
                {
                    return GetValueScale(stormElementData.Value, partSpan, heroesData, xmlParts);
                }
            }
        }

        return new ValueScale(0);
    }

    private static ValueScale GetValueScale(ReadOnlySpan<char> stormElementDataValue, ReadOnlySpan<char> partSpan, HeroesData heroesData, Span<Range> xmlParts)
    {
        if (double.TryParse(stormElementDataValue, out double dataValue))
        {
            if (heroesData.TryGetLevelScalingValue(partSpan[xmlParts[0]], partSpan[xmlParts[1]], partSpan[xmlParts[2]], out StormStringValue? stormStringValue))
            {
                if (double.TryParse(stormStringValue.Value, out double scalingValue))
                {
                    return new ValueScale(dataValue, scalingValue);
                }
            }
            else
            {
                return new ValueScale(dataValue);
            }
        }

        return new ValueScale(0);
    }

    private static int GetSizeOfBuffer(ReadOnlySpan<char> buffer, List<TextSection> dataRefParts)
    {
        int size = 0;

        foreach (TextSection dataSection in dataRefParts)
        {
            if (dataSection.IsText)
                size += buffer[dataSection.Range.Value].Length;
            else if (dataSection.IsValue)
                size += GameStringParser.MaxNumberLength;
        }

        return size;
    }

    private static ValueScale CalculateExpression(ReadOnlySpan<char> buffer, List<TextSection> dataRefParts)
    {
        int size = GetSizeOfBuffer(buffer, dataRefParts);
        int currentOffset = 0;
        double? scaleValue = null;

        Span<char> newBufferSpan = stackalloc char[size];

        foreach (TextSection dataSection in dataRefParts)
        {
            if (dataSection.IsText)
            {
                ReadOnlySpan<char> itemText = buffer[dataSection.Range.Value];

                itemText.CopyTo(newBufferSpan[currentOffset..]);
                currentOffset += itemText.Length;
            }
            else if (dataSection.IsValue)
            {
                dataSection.ValueScale.Value.Value.TryFormat(newBufferSpan[currentOffset..], out int charsWritten);
                currentOffset += charsWritten;

                // only get the first one
                if (!scaleValue.HasValue && dataSection.ValueScale.Value.Scaling.HasValue)
                    scaleValue = dataSection.ValueScale.Value.Scaling.Value;
            }
        }

        if (scaleValue is null)
            return new ValueScale(HeroesCalculator.Compute(newBufferSpan.TrimEnd('\0')));
        else
            return new ValueScale(HeroesCalculator.Compute(newBufferSpan.TrimEnd('\0')), scaleValue.Value);
    }
}
