using System.Text;
using U8Xml;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data that is parsed from an <see cref="XmlObject"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElementData
{
    private static readonly HashSet<string> _otherElementArrays = ["On", "Cost", "CatalogModifications", "ConditionalEvents", "CardLayouts"];
#if NET9_0_OR_GREATER
    private static readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _otherElementArraysAltLookup = _otherElementArrays.GetAlternateLookup<ReadOnlySpan<char>>();
#endif

    private string? _rawValue;
    private string? _constValue; // starts with $
    private string? _assetValue; // starts with @

    internal StormElementData(XmlNode rootElement)
    {
#if NET9_0_OR_GREATER
        ElementDataPairsAltLookup = ElementDataPairs.GetAlternateLookup<ReadOnlySpan<char>>();
        HashSet<string>.AlternateLookup<ReadOnlySpan<char>> a = _otherElementArrays.GetAlternateLookup<ReadOnlySpan<char>>();
#endif

        Parse(rootElement);
    }

    internal StormElementData(StormElementData parent, string field, bool isArray = false)
    {
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
    }

    internal StormElementData(StormElementData parent, string field, XmlNode rootElement)
        : this(parent, field)
    {
        Parse(rootElement);
    }

    internal StormElementData(StormElementData parent, string field, string value, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        _rawValue = value;
    }

    internal StormElementData(StormElementData parent, string field, string value, string? constValue, string? assetValue)
        : this(parent, field)
    {
        _rawValue = value;
        _constValue = constValue;
        _assetValue = assetValue;
    }

    internal StormElementData(StormElementData parent, string field, XmlNode rootElement, bool isInnerArray = false, bool isIndex = false)
        : this(parent, field, isIndex)
    {
        Parse(rootElement, isInnerArray);
    }

    internal StormElementData(StormElementData parent, string field, XmlNode element, string index, bool isInnerArray = false)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, element, isInnerArray, true);
    }

    internal StormElementData(StormElementData parent, string field, XmlAttribute attribute, string index)
        : this(parent, field)
    {
        ElementDataPairs[index] = new StormElementData(this, index, attribute.Value.ToString(), true);
    }

    /// <summary>
    /// Gets the parent data.
    /// </summary>
    public StormElementData? Parent { get; }

    /// <summary>
    /// Gets the representation of the current data reference field.
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is not <see langword="null"/>.
    /// </summary>
    public bool HasValue => !Value.IsNull;

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is evaluated from a const.
    /// </summary>
    public bool IsConstValue
    {
        get
        {
            if (_constValue is not null)
            {
                return true;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.IsConstValue)
                {
                    return data.IsConstValue;
                }
                else if (HasTextIndex)
                {
                    return ElementDataPairs.First().Value.IsConstValue;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is evaluated from an asset.
    /// </summary>
    public bool IsAssetValue
    {
        get
        {
            if (_assetValue is not null)
            {
                return true;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.IsAssetValue)
                {
                    return data.IsAssetValue;
                }
                else if (HasTextIndex)
                {
                    return ElementDataPairs.First().Value.IsAssetValue;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="HxdScaleValue"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(HxdScaleValue))]
    public bool HasHxdScale => ElementDataPairs.Count == 1 && ElementDataPairs.ContainsKey(ScaleValueParser.ScaleAttributeName);

    /// <summary>
    /// Gets the original value which represents a value of an <see cref="XAttribute"/>.
    /// </summary>
    public string? RawValue
    {
        get
        {
            if (_rawValue is not null)
            {
                return _rawValue;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                {
                    return data.RawValue;
                }
                else if (HasTextIndex)
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
    /// Gets the evaluated value of <see cref="RawValue"/>. For example, if it contains a const value ($var) or an element replacement (##name##), it will be updated with the new value.
    /// To get the original value, use <see cref="RawValue"/>.
    /// </summary>
    public StormElementValue Value
    {
        get
        {
            string? returnValue = null;

            if (_constValue is not null)
            {
                returnValue = _constValue;
            }
            else if (_assetValue is not null)
            {
                returnValue = _assetValue;
            }
            else if (_rawValue is not null)
            {
                returnValue = _rawValue;
            }
            else if (ElementDataPairs.Keys.Count == 1)
            {
                if (HasNumericalIndex && ElementDataPairs.TryGetValue("0", out StormElementData? data) && data.HasValue)
                    returnValue = data.Value.GetString();
                else if (HasTextIndex)
                    returnValue = ElementDataPairs.First().Value.Value.GetString();
            }

            return new StormElementValue(this)
            {
                Value = returnValue,
                IsNull = returnValue is null,
            };
        }
    }

    /// <summary>
    /// Gets the scaling value.
    /// </summary>
    public StormElementValue HxdScaleValue
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
    /// Gets a value indicating whether the inner data, <see cref="ElementDataPairs"/> consists of numerical keys.
    /// </summary>
    public bool HasNumericalIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether the inner data, <see cref="ElementDataPairs"/> consists of text keys.
    /// </summary>
    public bool HasTextIndex { get; init; }

    /// <summary>
    /// Gets the amount of elements.
    /// </summary>
    public int ElementDataCount => ElementDataPairs.Count;

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
            string display = $"Count = {ElementDataPairs.Count}";

            if (HasValue)
            {
                return $"Value = \"{Value.GetString()}\", {display}";
            }
            else
            {
                if (HasNumericalIndex)
                    return $"{display}, IsNumericalIndex";
                else if (HasTextIndex)
                    return $"{display}, IsTextIndex";
                else
                    return display;
            }
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

    internal void AddElement(XmlNode element, bool isInnerArray = false)
    {
        Parse(element, isInnerArray);
    }

    private void Parse(XmlNode rootElement, bool isInnerArray = false)
    {
        ProcessAttributes(rootElement, isInnerArray);

        XmlNodeList elements = rootElement.Children;

        foreach (XmlNode element in elements)
        {
            ProcessElement(element, isInnerArray);
        }
    }

    private void ProcessElement(XmlNode element, bool isInnerArray)
    {
        RawString elementName = element.Name;

        Span<char> buffer = stackalloc char[elementName.GetCharCount()];
        Encoding.UTF8.TryGetChars(elementName.AsSpan(), buffer, out int charsWritten);
        ReadOnlySpan<char> elementNameSpan = buffer;

        if (element.TryFindAttribute("index", out XmlAttribute indexAttribute) || element.TryFindAttribute("Index", out indexAttribute))
        {
            ProcessIndexAttribute(element, elementNameSpan, indexAttribute.Value);
        }
        else if (isInnerArray ||
            elementNameSpan.EndsWith("array", StringComparison.OrdinalIgnoreCase) ||
#if NET9_0_OR_GREATER
            _otherElementArraysAltLookup.Contains(elementNameSpan))
#else
            _otherElementArrays.Contains(elementName.ToString()))
#endif
        {
            if (TryGetElementDataAt(elementNameSpan, out StormElementData? existingData))
            {
                string nextIndex = existingData.ElementDataPairs.Keys.Count.ToString();
                existingData.ElementDataPairs[nextIndex] = new StormElementData(existingData, nextIndex, element, true, true);
            }
            else
            {
                string elementNameString = elementNameSpan.ToString();
                ElementDataPairs[elementNameString] = new StormElementData(this, elementNameString, element, "0", true)
                {
                    HasNumericalIndex = true,
                };
            }
        }
        else if (TryGetElementDataAt(elementNameSpan, out StormElementData? existingData))
        {
            existingData.AddElement(element);
        }
        else if (!(element.TryFindAttribute("value", out XmlAttribute valueAttribute) || element.TryFindAttribute("Value", out valueAttribute)))
        {
            string elementNameString = elementNameSpan.ToString();
            ElementDataPairs[elementNameString] = new StormElementData(this, elementNameString, element);
        }
        else
        {
            string? constValueAtt = null;
            string? assetValueAtt = null;

            if (element.TryFindAttribute($"{StormModStorage.SelfNameConst}value", out XmlAttribute constValueAttribute) ||
                element.TryFindAttribute($"{StormModStorage.SelfNameConst}Value", out constValueAttribute))
            {
                constValueAtt = constValueAttribute.Value.ToString();
            }

            if (element.TryFindAttribute($"{StormModStorage.SelfNameAsset}value", out XmlAttribute assetValueAttribute) ||
                element.TryFindAttribute($"{StormModStorage.SelfNameAsset}Value", out assetValueAttribute))
            {
                assetValueAtt = assetValueAttribute.Value.ToString();
            }

            string elementNameString = elementNameSpan.ToString();
            ElementDataPairs[elementNameString] = new StormElementData(this, elementNameString, valueAttribute.Value.ToString(), constValueAtt, assetValueAtt);
        }
    }

    private void ProcessAttributes(XmlNode rootElement, bool isInnerArray)
    {
        XmlAttributeList attributes = rootElement.Attributes;

        foreach (XmlAttribute attribute in attributes)
        {
            if (attribute.Name == "index" || attribute.Name == "Index")
                continue;

            if (attribute.Name == "value" || attribute.Name == "Value")
            {
                _rawValue = attribute.Value.ToString();
                continue;
            }

            if (attribute.Name == $"{StormModStorage.SelfNameConst}value" || attribute.Name == $"{StormModStorage.SelfNameConst}Value")
            {
                _constValue = attribute.Value.ToString();
                continue;
            }

            if (attribute.Name == $"{StormModStorage.SelfNameAsset}value" || attribute.Name == $"{StormModStorage.SelfNameAsset}Value")
            {
                _assetValue = attribute.Value.ToString();
                continue;
            }

            string attributeName = attribute.Name.ToString();

            if (isInnerArray)
            {
                ElementDataPairs[attributeName] = new StormElementData(this, attributeName, attribute, "0")
                {
                    HasNumericalIndex = true,
                };
            }
            else
            {
                ElementDataPairs[attributeName] = new StormElementData(this, attributeName, attribute.Value.ToString());
            }
        }
    }

    private void ProcessIndexAttribute(XmlNode element, ReadOnlySpan<char> elementNameSpan, RawString index)
    {
        Span<char> indexSpan = stackalloc char[index.GetCharCount()];
        Encoding.UTF8.TryGetChars(index.AsSpan(), indexSpan, out int charsWritten);

        if (TryGetElementDataAt(elementNameSpan, out StormElementData? existingElementData))
        {
            if (existingElementData.TryGetElementDataAt(indexSpan, out StormElementData? existingIndexedData))
            {
                existingIndexedData.AddElement(element, true);
            }
            else
            {
                string indexValue = index.ToString();
                existingElementData.ElementDataPairs[indexValue] = new StormElementData(existingElementData, indexValue, element, true, true);
            }
        }
        else
        {
            bool numericalIndex = index.TryToInt32(out int _);

            string elementNameString = elementNameSpan.ToString();
            ElementDataPairs[elementNameString] = new StormElementData(this, elementNameString, element, indexSpan.ToString(), true)
            {
                HasNumericalIndex = numericalIndex,
                HasTextIndex = !numericalIndex,
            };
        }
    }
}
