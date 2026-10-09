namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a top level XML element.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormXmlElement
{
    private readonly List<StormFile> _stormFiles = [];
    private readonly StormXmlMember _stormXmlMembers;

    internal StormXmlElement(StormXmlMember stormXmlMember, StormFile stormFile)
    {
        _stormXmlMembers = stormXmlMember;
        _stormFiles.Add(stormFile);

        Initialize(stormXmlMember);
        InitializeAttributes(stormXmlMember);
    }

    /// <summary>
    /// Gets the members of the XML element.
    /// </summary>
    public StormXmlMember Members => _stormXmlMembers;

    /// <summary>
    /// Gets the value of the <c>id</c> attribute.
    /// </summary>
    public virtual string? Id { get; private set; }

    /// <summary>
    /// Gets the value of the <c>parent</c> attribute.
    /// </summary>
    public string? Parent { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the <c>default</c> attribute is set to <c>1</c>.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Gets the name of the XML element.
    /// </summary>
    public string ElementName { get; private protected set; }

    /// <summary>
    /// Gets the type of the XML element.
    /// </summary>
    public StormXmlElementType ElementType { get; private protected set; }

    /// <summary>
    /// Gets a list of <see cref="StormFile"/>s that the element exists in.
    /// <para>
    /// The list will contain more than one item if the element has been merged from multiple <see cref="StormXmlElement"/>s.
    /// </para>
    /// </summary>
    public IReadOnlyList<StormFile> StormFiles => _stormFiles.AsReadOnly();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string id = string.IsNullOrWhiteSpace(Id) ? string.Empty : $", Id = \"{Id}\"";
            string parent = string.IsNullOrWhiteSpace(Parent) ? string.Empty : $", Parent = \"{Parent}\"";

            return $"{{{ElementName}{id}{parent}, Elements = {_stormXmlMembers.Count}}}";
        }
    }

    /// <summary>
    /// Merges the properties of another <see cref="StormXmlElement"/> into this instance.
    /// </summary>
    /// <param name="other">The <see cref="StormXmlElement"/> to merge into the current instance.</param>
    internal void Merge(StormXmlElement other)
    {
        if (ReferenceEquals(this, other))
            return;

        _stormXmlMembers.Merge(other._stormXmlMembers);
        _stormFiles.AddRange(other._stormFiles);

        Initialize(other._stormXmlMembers);
        InitializeAttributes(other._stormXmlMembers);
    }

    [MemberNotNull(nameof(ElementName))]
    private protected virtual void Initialize(StormXmlMember stormXmlProperty)
    {
        ElementName = stormXmlProperty.Name[1..];

        if (stormXmlProperty.Name.StartsWith('C'))
            ElementType = StormXmlElementType.Class;
        else if (stormXmlProperty.Name.StartsWith('S'))
            ElementType = StormXmlElementType.Struct;
        else if (stormXmlProperty.Name.StartsWith('E'))
            ElementType = StormXmlElementType.Enum;
        else
            ElementType = StormXmlElementType.Unknown;
    }

    private void InitializeAttributes(StormXmlMember stormXmlProperty)
    {
        Id = stormXmlProperty.TryGetMember("id", out StormXmlArray? idProperty) ? idProperty.Value : null;
        Parent = stormXmlProperty.TryGetMember("parent", out StormXmlArray? parentProperty) ? parentProperty.Value : null;
        IsDefault = stormXmlProperty.TryGetMember("default", out StormXmlArray? defaultProperty) && defaultProperty.Value is not null && defaultProperty.Value == "1";
    }
}
