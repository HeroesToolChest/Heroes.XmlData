using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data for an <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElement : IXmlData
{
    private const string IdAttribute = "id";
    private const string ParentAttribute = "parent";
    private readonly List<StormXElementValuePath> _originalStormXElementValues = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXElementValuePath"/>.</param>
    public StormElement(StormXElementValuePath baseValue)
    {
        _originalStormXElementValues.Add(baseValue);
        ElementType = baseValue.Value.Name.LocalName;

        DataValues = new StormElementData(baseValue.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormElement"/> class.
    /// </summary>
    /// <param name="baseValue">Another <see cref="StormElement"/> instance.</param>
    public StormElement(StormElement baseValue)
    {
        _originalStormXElementValues = [.. baseValue.OriginalStormXElementValues];
        DataValues = new StormElementData(OriginalStormXElementValues[0].Value);

        for (int i = 1; i < OriginalStormXElementValues.Count; i++)
        {
            DataValues.AddXElement(OriginalStormXElementValues[i].Value);
        }

        ElementType = baseValue.ElementType;
    }

    /// <summary>
    /// Gets a collection of all the original <see cref="XElement"/>s.
    /// </summary>
    public IReadOnlyList<StormXElementValuePath> OriginalStormXElementValues => _originalStormXElementValues.AsReadOnly();

    /// <summary>
    /// Gets the data values.
    /// </summary>
    public StormElementData DataValues { get; private set; }

    /// <summary>
    /// Gets the element type.
    /// </summary>
    public string ElementType { get; }

    /// <summary>
    /// Gets the value of the id attribute.
    /// </summary>
    public string? Id
    {
        get
        {
            if (HasId)
                return DataValues.ElementDataPairs[IdAttribute].Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets the value of the parent attribute.
    /// </summary>
    public string? ParentId
    {
        get
        {
            if (HasParentId)
                return DataValues.ElementDataPairs[ParentAttribute].Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Id"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Id))]
    public bool HasId => DataValues.ElementDataPairs.ContainsKey(IdAttribute);

    /// <summary>
    /// Gets a value indicating whether <see cref="ParentId"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ParentId))]
    public bool HasParentId => DataValues.ElementDataPairs.ContainsKey(ParentAttribute);

    /// <inheritdoc/>
    public int XmlDataCount => DataValues.ElementDataPairs.Count;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display;

            if (HasId && HasParentId)
                display = $"<{ElementType} id=\"{Id}\" parent=\"{ParentId}\">";
            else if (HasId && !HasParentId)
                display = $"<{ElementType} id=\"{Id}\">";
            else if (!HasId && HasParentId)
                display = $"<{ElementType} parent=\"{ParentId}\">";
            else
                display = $"<{ElementType}>";

            return display;
        }
    }

    /// <inheritdoc/>
    public StormElementData GetXmlData(ReadOnlySpan<char> index)
    {
        return DataValues.GetXmlData(index);
    }

    /// <inheritdoc/>
    public StormElementData GetXmlData(string index)
    {
        return DataValues.GetXmlData(index);
    }

    /// <inheritdoc/>
    public bool TryGetXmlData(ReadOnlySpan<char> index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        return DataValues.TryGetXmlData(index, out stormElementData);
    }

    /// <inheritdoc/>
    public bool TryGetXmlData(string index, [NotNullWhen(true)] out StormElementData? stormElementData)
    {
        return DataValues.TryGetXmlData(index.ToString(), out stormElementData);
    }

    /// <inheritdoc/>
    public IEnumerable<StormElementData> GetXmlData()
    {
        foreach (StormElementData data in DataValues.ElementDataPairs.Values)
        {
            yield return data;
        }
    }

    /// <summary>
    /// Merges a <see cref="XElement"/> into this current <see cref="StormElement"/>.
    /// </summary>
    /// <param name="stormXElementValue">An <see cref="XElement"/> along with its file path.</param>
    public void AddValue(StormXElementValuePath stormXElementValue)
    {
        _originalStormXElementValues.Add(stormXElementValue);

        DataValues.AddXElement(stormXElementValue.Value);
    }

    /// <summary>
    /// Merges another <see cref="StormElement"/> in this current <see cref="StormElement"/>.
    /// </summary>
    /// <param name="stormElement">Another <see cref="StormElement"/>.</param>
    public void AddValue(StormElement stormElement)
    {
        _originalStormXElementValues.AddRange(stormElement.OriginalStormXElementValues);

        foreach (StormXElementValuePath item in stormElement.OriginalStormXElementValues)
        {
            DataValues.AddXElement(item.Value);
        }
    }
}
