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
    /// Gets the name of the XML element (no type letter declaration).
    /// </summary>
    public string ElementName { get; private protected set; }

    /// <summary>
    /// Gets the name of the XML element (with type letter declaration, if available).
    /// </summary>
    public string ElementNameWithDeclaration { get; private protected set; }

    /// <summary>
    /// Gets the type of the XML element.
    /// </summary>
    public StormXmlElementType ElementType { get; private protected set; }

    /// <summary>
    /// Gets a value indicating whether the XML element has an <c>id</c> attribute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Id))]
    public bool HasId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the XML element has a <c>parent</c> attribute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Parent))]
    public bool HasParent { get; private set; }

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
            string id = HasId ? $", Id = \"{Id}\"" : string.Empty;
            string parent = HasParent ? $", Parent = \"{Parent}\"" : string.Empty;

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
    [MemberNotNull(nameof(ElementNameWithDeclaration))]
    private protected virtual void Initialize(StormXmlMember stormXmlMember)
    {
        ElementName = stormXmlMember.Name[1..];
        ElementNameWithDeclaration = stormXmlMember.Name;

        if (stormXmlMember.Name.StartsWith('C'))
            ElementType = StormXmlElementType.Class;
        else if (stormXmlMember.Name.StartsWith('S'))
            ElementType = StormXmlElementType.Struct;
        else if (stormXmlMember.Name.StartsWith('E'))
            ElementType = StormXmlElementType.Enum;
        else
            ElementType = StormXmlElementType.Unknown;
    }

    private void InitializeAttributes(StormXmlMember stormXmlMember)
    {
        Id = stormXmlMember.TryGetMember("id", out StormXmlArray? idProperty) ? idProperty.Value : null;
        Parent = stormXmlMember.TryGetMember("parent", out StormXmlArray? parentProperty) ? parentProperty.Value : null;
        IsDefault = stormXmlMember.TryGetMember("default", out StormXmlArray? defaultProperty) && defaultProperty.Value.HasValue && defaultProperty.Value == "1";

        HasId = !string.IsNullOrWhiteSpace(Id);
        HasParent = !string.IsNullOrWhiteSpace(Parent);
    }
}
