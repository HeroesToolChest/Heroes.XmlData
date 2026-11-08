namespace Heroes.StormData.GameStrings;

internal static class DataRefFieldParser
{
    // the names of the game's enums used as named indexes, in ordinal order.
    // a numerical index into an array that uses these names is the ordinal, e.g. VitalMaxArray[1] is VitalMaxArray[Shields]
    private static readonly string[][] _namedIndexEnums =
    [
        ["Life", "Shields", "Energy"],
    ];

    // Abil,GuldanHorrify,CastIntroTime
    // Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    public static FieldNode ParseField(StormXmlElement stormXmlElement, ReadOnlySpan<char> field)
    {
        FieldNode currentNode = new(stormXmlElement.Members);

        // the split enumerator walks the segments without a buffer for them
        foreach (Range fieldSegmentRange in field.Split('.'))
        {
            ReadOnlySpan<char> fieldSegment = field[fieldSegmentRange].Trim();
            if (fieldSegment.IsEmpty)
                continue;

            currentNode = ParseFieldSegment(currentNode, fieldSegment);
            if (currentNode.IsEmpty)
                return default;
        }

        return currentNode;
    }

    // -> CastIntroTime
    // -> PeriodicPeriodArray[0]
    private static FieldNode ParseFieldSegment(FieldNode currentNode, ReadOnlySpan<char> fieldSegment)
    {
        if (fieldSegment[^1] == ']')
        {
            // PeriodicPeriodArray[0]
            int indexOfStartBracket = fieldSegment.LastIndexOf("[");
            if (indexOfStartBracket < 1)
                return default;

            ReadOnlySpan<char> name = fieldSegment[..indexOfStartBracket]; // the part without the indexer, e.g. PeriodicPeriodArray
            ReadOnlySpan<char> index = fieldSegment[(indexOfStartBracket + 1)..^1]; // the value of the index, e.g. 0

            if (currentNode.TryGetMember(name, out StormXmlArray? nameArray))
            {
                if (nameArray.IsArray && TryGetByIndex(nameArray, index, out StormXmlMember? member))
                {
                    return new FieldNode(member);
                }
                else
                {
                    return new FieldNode(nameArray);
                }
            }

            return default;
        }

        if (!currentNode.TryGetMember(fieldSegment, out StormXmlArray? stormXmlArray))
            return default;

        // array with no indexer, e.g. AmountArray: use the first entry
        if (stormXmlArray.IsArray)
        {
            if (stormXmlArray.TryGetByIndex(0, out StormXmlMember? first))
                return new FieldNode(first);

            return default;
        }

        return new FieldNode(stormXmlArray);
    }

    private static bool TryGetByIndex(StormXmlArray array, ReadOnlySpan<char> index, [NotNullWhen(true)] out StormXmlMember? member)
    {
        // the position of a named index in the xml is not its ordinal, so map the number to the enum name instead
        if (int.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out int ordinal))
        {
            foreach (string[] enumNames in _namedIndexEnums)
            {
                if (!ContainsAnyIndex(array, enumNames))
                    continue;

                member = null;
                return ordinal < enumNames.Length && array.TryGetByIndex(enumNames[ordinal], out member);
            }
        }

        return array.TryGetByIndex(index, out member);
    }

    private static bool ContainsAnyIndex(StormXmlArray array, string[] names)
    {
        foreach (string name in names)
        {
            if (array.ContainsIndex(name))
                return true;
        }

        return false;
    }
}
