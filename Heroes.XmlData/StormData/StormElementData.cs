namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data that represents an <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormElementData
{
    private static readonly HashSet<string> _otherElementArrays = new(StringComparer.OrdinalIgnoreCase)
    {
        "CatalogModifications",
        "CardLayouts",
        "ConditionalEvents",
        "LayoutButtons",
        "On",
        "Remove",
        "TooltipAppender",
        "DurationOverride",
        "Buttons",
    };

    // elements arrays that should only be arrays for the given element type
    private static readonly Dictionary<string, string> _elementTypeByElementArray = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Cost", "CEffectModifyUnit" },
    };

    private string? _value;
    private int _currentIndex = 0;

    internal StormElementData(StormElement stormElement, XElement rootElement)
    {
        StormElement = stormElement;

#if NET9_0_OR_GREATER
        ElementDataPairsAltLookup = ElementDataPairs.GetAlternateLookup<ReadOnlySpan<char>>();
#endif

        Parse(rootElement);
    }

    private StormElementData(StormElementData parent, string field, bool isArray = false)
    {
        StormElement = parent.StormElement;

#if NET9_0_OR_GREATER
        ElementDataPairsAltLookup = ElementDataPairs.GetAlternateLookup<ReadOnlySpan<char>>();
#endif

        Parent = parent;

        if (!string.IsNullOrWhiteSpace(parent.Field))
        {
            if (isArray)
                Field = $"{parent.Field}[{field}]";
            else
                Field = $"{parent.Field}.{field}";
        }
        else
        {
            Field = field;
        }

        IsIndexed = isArray;
    }

    private StormElementData(StormElementData parent, string field, XElement rootElement)
        : this(parent, field)
    {
        Parse(rootElement);
    }

    private StormElementData(StormElementData parent, string field, string value, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        _value = value;
    }

    private StormElementData(StormElementData parent, string field, XElement rootElement, bool isInnerArray = false, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        Parse(rootElement, isInnerArray);
    }

    private StormElementData(StormElementData parent, string field, XElement element, string index, bool isInnerArray = false)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, element, isInnerArray, true);
    }

    private StormElementData(StormElementData parent, string field, XAttribute attribute, string index)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, attribute.Value, true);
    }

    /// <summary>
    /// Gets the main storm element that this data belongs to.
    /// </summary>
    public StormElement StormElement { get; }

    /// <summary>
    /// Gets the parent data.
    /// </summary>
    public StormElementData? Parent { get; }

    /// <summary>
    /// Gets a value indicating whether this data is indexed (is in an array).
    /// </summary>
    public bool IsIndexed { get; }

    /// <summary>
    /// Gets the representation of the current data reference field.
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is not <see langword="null"/>.
    /// </summary>
    public bool HasValue => !Value.IsNull;

    /// <summary>
    /// Gets the original value which represents a value of an <see cref="XAttribute"/>.
    /// </summary>
    public string? RawValue
    {
        get
        {
            if (_value is not null)
            {
                return _value;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    return data.RawValue;
                }
                else
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    if (firstElementData.HasValue)
                        return firstElementData.RawValue;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the evaluated value of <see cref="RawValue"/>. For example, if it contains an element replacement (##name##), it will be updated with the new value.
    /// To get the original value, use <see cref="RawValue"/>.
    /// </summary>
    public StormElementValue Value
    {
        get
        {
            ReadOnlySpan<char> returnValue = null;
            bool isNull = false;

            if (_value is not null)
            {
                returnValue = _value;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    isNull = data.Value.IsNull;
                    returnValue = data.Value.Value;
                }
                else
                {
                    StormElementData firstElementData = ElementDataPairs.First().Value;
                    isNull = firstElementData.Value.IsNull;
                    returnValue = firstElementData.Value.Value;
                }
            }

            return new StormElementValue(this)
            {
                Value = returnValue,
                IsNull = isNull,
            };
        }
    }

    /// <summary>
    /// Gets the amount of elements.
    /// </summary>
    public int ElementDataCount => ElementDataPairs.Count;

    /// <summary>
    /// Gets a value indicating whether <see cref="HxdScaleValue"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(HxdScaleValue))]
    internal bool HasHxdScale => ElementDataPairs.Count == 1 && ElementDataPairs.ContainsKey(ScaleValueParser.ScaleAttributeName);

    /// <summary>
    /// Gets the scaling value.
    /// </summary>
    internal StormElementValue HxdScaleValue
    {
        get
        {
            if (HasHxdScale)
            {
                return new StormElementValue(this)
                {
                    Value = ElementDataPairs[ScaleValueParser.ScaleAttributeName].RawValue,
                    IsNull = false,
                };
            }

            return new StormElementValue(this)
            {
                IsNull = true,
            };
        }
    }

    /// <summary>
    /// Gets the inner data.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal Dictionary<string, StormElementData> ElementDataPairs { get; } = new(StringComparer.OrdinalIgnoreCase);

#if NET9_0_OR_GREATER
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal Dictionary<string, StormElementData>.AlternateLookup<ReadOnlySpan<char>> ElementDataPairsAltLookup { get; }
#endif

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Field))
                return $"{{ROOT, Elements = {ElementDataCount}}}";
            else if (!string.IsNullOrWhiteSpace(Value.GetString()))
                return $"{{\"{Field}\", RawValue = \"{RawValue}\", Elements = {ElementDataCount}}}";
            else
                return $"{{\"{Field}\", Elements = {ElementDataCount}}}";
        }
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData this[string index]
    {
        get
        {
            return GetElementDataAt(index);
        }
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData this[ReadOnlySpan<char> index]
    {
        get
        {
            return GetElementDataAt(index);
        }
    }

    /// <summary>
    /// Gets a collection of the inner data indexes.
    /// </summary>
    /// <returns>A collection of the inner data indexes.</returns>
    public IEnumerable<string> GetElementDataIndexes()
    {
        return ElementDataPairs.Keys;
    }

    /// <summary>
    /// Gets a collection of the inner data elements.
    /// </summary>
    /// <returns>A collection of <see cref="KeyValuePair"/>s representing the inner data elements.</returns>
    public IEnumerable<KeyValuePair<string, StormElementData>> GetElementData()
    {
        return ElementDataPairs.AsReadOnly();
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData GetElementDataAt(ReadOnlySpan<char> index)
    {
#if NET9_0_OR_GREATER
        if (ElementDataPairsAltLookup.TryGetValue(index, out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException($"Value '{index}' was not found.");
#else
        return GetElementDataAt(index.ToString());
#endif
    }

    /// <summary>
    /// Gets the inner xml data from the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns>The inner xml data as <see cref="StormElementData"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    /// <exception cref="KeyNotFoundException"><paramref name="index"/> was not found.</exception>
    public StormElementData GetElementDataAt(string index)
    {
        ArgumentNullException.ThrowIfNull(index);

        if (ElementDataPairs.TryGetValue(index, out StormElementData? stormElementData))
            return stormElementData;

        throw new KeyNotFoundException($"Value '{index}' was not found.");
    }

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    public bool TryGetElementDataAt(ReadOnlySpan<char> index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
#if NET9_0_OR_GREATER
        return ElementDataPairsAltLookup.TryGetValue(index, out stormElementData);
#else
        return TryGetElementDataAt(index.ToString(), out stormElementData);
#endif
    }

    /// <summary>
    /// Looks up the inner xml data from the given <paramref name="index"/>, returning a value that indicates whether such value exists.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <param name="stormElementData">The returming <see cref="StormElementData"/> if <paramref name="index"/> is found.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    public bool TryGetElementDataAt(string index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        ArgumentNullException.ThrowIfNull(index);

        return ElementDataPairs.TryGetValue(index, out stormElementData);
    }

    /// <summary>
    /// Determines whether the inner data contains the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">A character span that contains the index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    public bool ContainsIndex(ReadOnlySpan<char> index)
    {
#if NET9_0_OR_GREATER
        return ElementDataPairsAltLookup.ContainsKey(index);
#else
        return ContainsIndex(index.ToString());
#endif
    }

    /// <summary>
    /// Determines whether the inner data contains the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index value which is an element name or attribute name or value. Is case-insensitive.</param>
    /// <returns><see langword="true"/> if the index is found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null"/>.</exception>
    public bool ContainsIndex(string index)
    {
        ArgumentNullException.ThrowIfNull(index);

        return ElementDataPairs.ContainsKey(index);
    }

    internal IEnumerable<StormElementData> GetElements()
    {
        foreach (StormElementData data in ElementDataPairs.Values)
        {
            if (data.ElementDataPairs.Count == 0)
            {
                yield return data;
            }
            else
            {
                foreach (StormElementData innerData in data.GetElements())
                {
                    yield return innerData;
                }
            }
        }
    }

    internal void AddXElement(XElement element, bool isInnerArray = false)
    {
        Parse(element, isInnerArray);
    }

    private void Parse(XElement rootElement, bool isInnerArray = false)
    {
        IEnumerable<XAttribute> attributes = rootElement.Attributes();
        IEnumerable<XElement> elements = rootElement.Elements();

        foreach (XAttribute attribute in attributes)
        {
            if (attribute.Name.LocalName.Equals("index", StringComparison.OrdinalIgnoreCase))
                continue;

            if (attribute.Name.LocalName.Equals("removed", StringComparison.OrdinalIgnoreCase))
                continue;

            if (attribute.Name.LocalName.Equals("default", StringComparison.OrdinalIgnoreCase))
                continue;

            if (attribute.Name.LocalName.Equals("value", StringComparison.OrdinalIgnoreCase))
            {
                _value = attribute.Value;
                continue;
            }

            if (isInnerArray)
            {
                ElementDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute, "0");
            }
            else if (attribute.Name.LocalName.Equals("parent", StringComparison.OrdinalIgnoreCase) &&
                ElementDataPairs.TryGetValue("id", out StormElementData? existingIdData) && !string.IsNullOrEmpty(existingIdData.RawValue) &&
                existingIdData.RawValue.Equals(attribute.Value))
            {
                // skip the parent attribute if it's the same as the existing id attribute.
                continue;
            }
            else
            {
                ElementDataPairs[attribute.Name.LocalName] = new StormElementData(this, attribute.Name.LocalName, attribute.Value);
            }
        }

        foreach (XElement element in elements)
        {
            string elementName = element.Name.LocalName;
            string? indexAtt = element.Attribute("index")?.Value ?? element.Attribute("Index")?.Value;
            string? valueAtt = element.Attribute("value")?.Value ?? element.Attribute("Value")?.Value;
            string? removedAtt = element.Attribute("removed")?.Value ?? element.Attribute("Removed")?.Value;

            bool isRemovedElement = !string.IsNullOrEmpty(removedAtt) && removedAtt.Equals("1");

            if (!string.IsNullOrEmpty(indexAtt))
            {
                ParseElementWithIndex(element, elementName, indexAtt, isRemovedElement);
            }
            else if (isInnerArray || elementName.AsSpan().EndsWith("array", StringComparison.OrdinalIgnoreCase) || _otherElementArrays.Contains(elementName) || IsElementArrayForCurrentType(elementName))
            {
                if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingData))
                {
                    existingData._currentIndex++;
                    string nextIndex = existingData._currentIndex.ToString();

                    existingData.ElementDataPairs[nextIndex] = new StormElementData(existingData, nextIndex, element, true, true);
                }
                else
                {
                    ElementDataPairs[elementName] = new StormElementData(this, elementName, element, "0", true);
                }
            }
            else if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingData))
            {
                existingData.AddXElement(element);
            }
            else if (string.IsNullOrEmpty(valueAtt))
            {
                ElementDataPairs[elementName] = new StormElementData(this, elementName, element);
            }
            else
            {
                ElementDataPairs[elementName] = new StormElementData(this, elementName, valueAtt);
            }
        }
    }

    private void ParseElementWithIndex(XElement element, string elementName, string indexAtt, bool isRemoved)
    {
        int? numericalIndex = int.TryParse(indexAtt, out int parsedIndex) ? parsedIndex : null;

        if (ElementDataPairs.TryGetValue(elementName, out StormElementData? existingElementData))
        {
            if (existingElementData.ElementDataPairs.TryGetValue(indexAtt, out StormElementData? existingIndexedData))
            {
                if (isRemoved)
                {
                    existingElementData.ElementDataPairs.Remove(indexAtt);
                    return;
                }

                existingIndexedData.AddXElement(element, true);
            }
            else
            {
                existingElementData._currentIndex = numericalIndex ?? existingElementData._currentIndex + 1;
                existingElementData.ElementDataPairs[indexAtt] = new StormElementData(existingElementData, indexAtt, element, true, true);
            }
        }
        else
        {
            _currentIndex = numericalIndex ?? _currentIndex + 1;
            ElementDataPairs[elementName] = new StormElementData(this, elementName, element, indexAtt, true);
        }
    }

    private bool IsElementArrayForCurrentType(string elementName)
    {
        if (_elementTypeByElementArray.TryGetValue(elementName, out string? elementType))
            return StormElement.ElementType.Equals(elementType, StringComparison.OrdinalIgnoreCase);

        return false;
    }
}
