namespace Heroes.StormData.Xml.Elements.Style;

/// <summary>
/// Represents a <c>Style</c> element from a <c>.stormstyle</c> file (e.g. <c>&lt;Style name="ReticleEnemy" template="Storm_Tutorial_Reticle_Text" textcolor="255,255,255,255" /&gt;</c>).
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormXmlStyleElement : StormXmlElement
{
    internal StormXmlStyleElement(StormXmlMember stormXmlMember, StormFile stormFile)
        : base(stormXmlMember, stormFile)
    {
    }

    private StormXmlStyleElement(StormXmlStyleElement other)
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

            throw new HeroesStormDataException($"The 'name' attribute is required for style elements. Element '{ElementName}' in file '{StormFiles[0].FullPath}' does not have a 'name' attribute.");
        }
    }

    /// <summary>
    /// Gets the value of the <c>template</c> attribute, or <see langword="null"/> if it does not exist.
    /// </summary>
    public string? Template => Members.TryGetMember(StormXmlConstants.TemplateAttributeName, out StormXmlArray? templateProperty) && templateProperty.Value.HasValue ? templateProperty.Value.RawValue : null;

    /// <summary>
    /// Gets a value indicating whether the element has a <c>template</c> attribute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Template))]
    public bool HasTemplate => Template is not null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string template = HasTemplate ? $", Template = \"{Template}\"" : string.Empty;

            return $"{{{ElementName}, Name = \"{Name}\"{template}, Members = {Members.Count}}}";
        }
    }

    internal override StormXmlElement Clone() => new StormXmlStyleElement(this);

    private protected override void Initialize(StormXmlMember stormXmlMember)
    {
        ElementName = stormXmlMember.Name;
        ElementNameWithDeclaration = ElementName;

        ElementType = StormXmlElementType.Style;
    }
}
