using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Represents an top level <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElement
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
                return DataValues.ElementDataPairs[IdAttribute].RawValue;
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
                return DataValues.ElementDataPairs[ParentAttribute].RawValue;
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

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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

    /// <summary>
    /// Gets a collection of all the inner elements and attributes.
    /// </summary>
    /// <returns>A collection of all the inner xml data as <see cref="StormElementData"/>.</returns>
    public IEnumerable<StormElementData> GetElements()
    {
        return DataValues.GetElements();
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
