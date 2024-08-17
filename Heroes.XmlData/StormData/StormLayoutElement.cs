using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data for a storm layout <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormLayoutElement : StormElement
{
    private const string NameAttribute = "name";
    private const string TypeAttribute = "type";

    /// <summary>
    /// Initializes a new instance of the <see cref="StormLayoutElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXElementValuePath"/>.</param>
    public StormLayoutElement(StormXElementValuePath baseValue)
        : base(baseValue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormLayoutElement"/> class.
    /// </summary>
    /// <param name="baseValue">Another <see cref="StormElement"/> instance.</param>
    public StormLayoutElement(StormElement baseValue)
        : base(baseValue)
    {
    }

    /// <summary>
    /// Gets the value of the name attribute.
    /// </summary>
    public string? Name
    {
        get
        {
            if (HasName)
                return DataValues.ElementDataPairs[NameAttribute].Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets the value of the type attribute.
    /// </summary>
    public string? Type
    {
        get
        {
            if (HasType)
                return DataValues.ElementDataPairs[TypeAttribute].Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Name"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Name))]
    public bool HasName => DataValues.ElementDataPairs.ContainsKey(NameAttribute);

    /// <summary>
    /// Gets a value indicating whether <see cref="Type"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Type))]
    public bool HasType => DataValues.ElementDataPairs.ContainsKey(TypeAttribute);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display;

            if (HasName && HasType)
                display = $"<{ElementType} name=\"{Name}\" template=\"{HasType}\">";
            else if (HasName && !HasType)
                display = $"<{ElementType} name=\"{Name}\">";
            else if (!HasName && HasType)
                display = $"<{ElementType} template=\"{HasType}\">";
            else
                display = $"<{ElementType}>";

            return display;
        }
    }
}
