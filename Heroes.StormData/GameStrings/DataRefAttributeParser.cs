using System.Runtime.InteropServices;

namespace Heroes.StormData.GameStrings;

internal sealed class DataRefAttributeParser
{
    private static readonly SearchValues<char> _gameStringOps = SearchValues.Create("+-*/()");

    private readonly LayeredDataStorage _layeredDataStorage;
    private readonly DataTagParser _dataTagParser;

    public DataRefAttributeParser(LayeredDataStorage layeredDataStorage, DataTagParser dataTagParser)
    {
        _layeredDataStorage = layeredDataStorage;
        _dataTagParser = dataTagParser;
    }

    // Abil,GuldanHorrify,CastIntroTime+Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    public ValueScale Parse(ReadOnlySpan<char> dRefSpan)
    {
        List<TextSectionItem> dataRefParts = [];
        int startIndex = 0;

        while (startIndex < dRefSpan.Length)
        {
            // getting the part of the expression, e.g. Abil,GuldanHorrify,CastIntroTime
            ReadOnlySpan<char> partSpan = GetNextPart(dRefSpan[startIndex..]);
            ReadOnlySpan<char> partSpanTrimmed = partSpan.Trim();

            if (!partSpanTrimmed.IsEmpty)
            {
                if (partSpanTrimmed[0] == '[')
                    dataRefParts.Add(new TextSectionItem(ParseBracketDRef(partSpanTrimmed)));
                else if (double.TryParse(partSpanTrimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    dataRefParts.Add(new TextSectionItem(new ValueScale(value))); // hardcoded value
                else if (partSpanTrimmed[0] == '$' && partSpanTrimmed[^1] == '$')
                    dataRefParts.Add(new TextSectionItem(new ValueScale(0)));
                else
                    dataRefParts.Add(new TextSectionItem(ParsePart(partSpan)));
            }

            if (startIndex + partSpan.Length >= dRefSpan.Length)
                break;

            // the operator
            dataRefParts.Add(new TextSectionItem(new Range(startIndex + partSpan.Length, startIndex + partSpan.Length + 1)));

            startIndex += partSpan.Length + 1;
        }

        return CalculateExpression(dRefSpan, dataRefParts);
    }

    private static ReadOnlySpan<char> GetNextPart(ReadOnlySpan<char> text)
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

                int scalingDigits = item.ValueScale.TotalScalingDigits();
                if (scalingDigits > 0)
                    scalingSize += scalingDigits + 1; // +1 for '*';
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

            destination[currentOffset..][0] = '(';
            currentOffset++;

            // this value needs to be wrapped in parenthesis
            valueScale.Value.TryFormat(destination[currentOffset..], out int charsWritten);
            currentOffset += charsWritten;

            if (includeScaling && valueScale.Scaling.HasValue)
            {
                destination[currentOffset..][0] = '*';
                currentOffset++;

                (valueScale.Scaling.Value + 1).TryFormat(destination[currentOffset..], out charsWritten);
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

        return _dataTagParser.ParseDataTag(temp);
    }

    // Abil,GuldanHorrify,CastIntroTime
    // Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    // Effect,AnduinHolyWordSalvationLightOfStormwindCooldownReduction,Cost[0].CooldownTimeUse
    private ValueScale ParsePart(ReadOnlySpan<char> fullPartSpan)
    {
        // a proper dref consists of <Catalog>.<Entry>.<Field>
        // <Field>s are comma separated
        int splitterCount = fullPartSpan.CountAny('.', ',');

        if (splitterCount < 2)
            return new ValueScale(0);

        Span<Range> xmlParts = stackalloc Range[splitterCount + 1];

        fullPartSpan.SplitAny(xmlParts, ".,", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        //return ParseStormElement(fullPartSpan, fullPartSpan[xmlParts[1]], xmlParts);
        return new ValueScale(0); // TODO: Implement the actual parsing logic for the dref part
    }

    private ValueScale CalculateExpression(ReadOnlySpan<char> dRefSpan, List<TextSectionItem> dataRefParts)
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

    private double ComputeExpression(ReadOnlySpan<char> dRefSpan, int bufferSize, List<TextSectionItem> dataRefParts, bool addScaling)
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
}
