namespace Heroes.StormData.GameStrings;

internal static class DataRefFieldParser
{
    // Abil,GuldanHorrify,CastIntroTime
    // Effect,GuldanHorrifyAbilityStartCreatePersistent,PeriodicPeriodArray[0]
    public static FieldNode ParseField(StormXmlElement stormXmlElement, ReadOnlySpan<char> field)
    {
        int splitterCount = field.CountAny('.');

        Span<Range> fieldSegmentRanges = stackalloc Range[splitterCount + 1];

        field.SplitAny(fieldSegmentRanges, ".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        FieldNode currentNode = new(stormXmlElement.Members);

        foreach (Range fieldSegmentRange in fieldSegmentRanges)
        {
            ReadOnlySpan<char> fieldSegment = field[fieldSegmentRange];
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

            //if (current.TryGetMember(name, out StormXmlArray? array) && array.TryGetByIndex(index, out StormXmlMember? member))
            //    return new FieldNode(member);
            if (currentNode.TryGetMember(name, out StormXmlArray? nameArray))
            {
                if (nameArray.IsArray && nameArray.TryGetByIndex(index, out StormXmlMember? member))
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

        // array with no indexer, e.g. AmountArray: use it only if it has a single entry
        if (stormXmlArray.IsArray)
        {
            if (!stormXmlArray.ContainsIndex(1) && stormXmlArray.TryGetByIndex(0, out StormXmlMember? only))
                return new FieldNode(only);

            return default;
        }

        return new FieldNode(stormXmlArray);
        //   ReadOnlySpan<char> fieldPartIndexValue = fieldPart.Slice(indexOfStartBracket + 1, fieldPart.Length - indexOfStartBracket - 2);
        //   bool numericalIndex = int.TryParse(fieldPartIndexValue, out int indexAsNumber);

        //    // the part without the indexer, e.g. PeriodicPeriodArray
        //    ReadOnlySpan<char> fieldPartSpanWithoutIndexer = fieldPart[..indexOfStartBracket];

        //    if (members.TryGetMember(fieldPartSpanWithoutIndexer, out StormXmlArray? stormXmlArray))
        //    {
        //        if (stormXmlArray.TryGetByIndex(fieldPartIndexValue, out StormXmlMember? indexStormXmlMember))
        //        {
        //            return (indexStormXmlMember, null);
        //        }
        //    }
        //    return null;
        //}
        //else if (members.TryGetMember(fieldPart, out StormXmlArray? stormXmlArray))
        //{
        //    if (stormXmlArray.IsArray)
        //    {

        //    }
        //    else
        //    {
        //        return (members, stormXmlArray);
        //    }
        //}
        //else
        //{
        //    return (members, null);
        //}

        //return null;
    }
}
