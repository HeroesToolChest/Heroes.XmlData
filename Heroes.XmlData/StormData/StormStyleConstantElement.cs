using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the for a storm style constant <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormStyleConstantElement : StormElement
{
    private const string NameAttribute = "name";
    private const string ValAttribute = "val";

    /// <summary>
    /// Initializes a new instance of the <see cref="StormStyleConstantElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXElementValuePath"/>.</param>
    public StormStyleConstantElement(StormXElementValuePath baseValue)
        : base(baseValue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormStyleConstantElement"/> class.
    /// </summary>
    /// <param name="baseValue">Another <see cref="StormElement"/> instance.</param>
    public StormStyleConstantElement(StormElement baseValue)
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
    /// Gets the value of the val attribute.
    /// </summary>
    public string? Val
    {
        get
        {
            if (HasVal)
                return DataValues.ElementDataPairs[ValAttribute].Value;
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
    /// Gets a value indicating whether <see cref="Val"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Val))]
    public bool HasVal => DataValues.ElementDataPairs.ContainsKey(ValAttribute);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display;

            if (HasName && HasVal)
                display = $"<{ElementType} name=\"{Name}\" val=\"{Val}\">";
            else if (HasName && !HasVal)
                display = $"<{ElementType} name=\"{Name}\">";
            else if (!HasName && HasVal)
                display = $"<{ElementType} val=\"{Val}\">";
            else
                display = $"<{ElementType}>";

            return display;
        }
    }
}
