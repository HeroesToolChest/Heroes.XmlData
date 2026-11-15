namespace Heroes.StormData.Xml.Elements.Style;

/// <summary>
/// Represents a <c>Constant</c> element from a <c>.stormstyle</c> file (e.g. <c>&lt;Constant name="ColorGray" val="a7a7a7" /&gt;</c>).
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormXmlStyleConstantElement : StormXmlElement
{
    internal StormXmlStyleConstantElement(StormXmlMember stormXmlMember, StormFile stormFile)
        : base(stormXmlMember, stormFile)
    {
    }

    private StormXmlStyleConstantElement(StormXmlStyleConstantElement other)
        : base(other)
    {
    }

    /// <summary>
    /// Gets the value of the <c>name</c> attribute.
    /// </summary>
    /// <remarks>Style elements are keyed by <c>name</c> rather than <c>id</c>, so this returns the same value as <see cref="Name"/>.</remarks>
    public override string Id => Name;

    /// <summary>
    /// Gets the value of the <c>name</c> attribute.
    /// </summary>
    /// <exception cref="HeroesStormDataException">Thrown when the element does not have a <c>name</c> attribute.</exception>
    public string Name
    {
        get
        {
            if (Members.TryGetMember(StormXmlConstants.NameAttributeName, out StormXmlArray? nameProperty) && nameProperty.Value.HasValue)
                return nameProperty.Value.RawValue;

            throw new HeroesStormDataException($"The 'name' attribute is required for style constant elements. Element '{ElementName}' in file '{StormFiles[0].FullPath}' does not have a 'name' attribute.");
        }
    }

    /// <summary>
    /// Gets the value of the <c>val</c> attribute, or an empty string if it does not exist.
    /// </summary>
    public string Value => Members.TryGetMember(StormXmlConstants.ValAttributeName, out StormXmlArray? valProperty) ? valProperty.Value.RawValue : string.Empty;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{{{ElementName}, Name = \"{Name}\", Value = \"{Value}\"}}";

    internal override StormXmlElement Clone() => new StormXmlStyleConstantElement(this);

    private protected override void Initialize(StormXmlMember stormXmlMember)
    {
        ElementName = stormXmlMember.Name;
        ElementNameWithDeclaration = ElementName;

        ElementType = StormXmlElementType.StyleConstant;
    }
}
