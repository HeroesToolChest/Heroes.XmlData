namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a top level XML element.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormXmlElement
{
    private readonly List<StormFile> _stormFiles = [];
    private readonly StormXmlMember _stormXmlMembers;
    private string? _id;

    internal StormXmlElement(StormXmlMember stormXmlMember, StormFile stormFile)
    {
        _stormXmlMembers = stormXmlMember;
        _stormFiles.Add(stormFile);

        Initialize(stormXmlMember);
        InitializeAttributes(stormXmlMember);
    }

    private protected StormXmlElement(StormXmlElement other)
    {
        _stormXmlMembers = other._stormXmlMembers.Clone();
        _stormFiles = [.. other._stormFiles];

        _id = other._id;
        Parent = other.Parent;
        IsDefault = other.IsDefault;
        HasId = other.HasId;
        HasParent = other.HasParent;
        ElementName = other.ElementName;
        ElementNameWithDeclaration = other.ElementNameWithDeclaration;
        ElementType = other.ElementType;
    }

    /// <summary>
    /// Gets the members of the XML element.
    /// </summary>
    public StormXmlMember Members => _stormXmlMembers;

    /// <summary>
    /// Gets the value of the <c>id</c> attribute.
    /// </summary>
    public virtual string? Id => _id;

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

            return $"{{{ElementName}{id}{parent}, Members = {_stormXmlMembers.Count}}}";
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

        string? previousParent = Parent;

        _stormXmlMembers.Merge(other._stormXmlMembers);
        _stormFiles.AddRange(other._stormFiles);

        Initialize(other._stormXmlMembers);
        InitializeAttributes(other._stormXmlMembers);

        // If the merging element's parent equals its own id (case-sensitive),
        // it is a self-referential parent and should be ignored; preserve the current element's parent.
        if (other.HasParent && other.HasId && string.Equals(other.Parent, other.Id, StringComparison.Ordinal))
        {
            Parent = previousParent;
            HasParent = !string.IsNullOrWhiteSpace(Parent);

            if (previousParent is not null)
                _stormXmlMembers.Add(StormXmlConstants.ParentAttributeName, previousParent, _stormXmlMembers);
            else
                _stormXmlMembers.Remove(StormXmlConstants.ParentAttributeName);
        }
    }

    /// <summary>
    /// Creates a deep copy of the current StormXmlElement.
    /// </summary>
    /// <returns>A new <see cref="StormXmlElement"/> instance that is a copy of the current element.</returns>
    internal virtual StormXmlElement Clone() => new(this);

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
        _id = stormXmlMember.TryGetMember(StormXmlConstants.IdAttributeName, out StormXmlArray? idProperty) ? idProperty.Value : null;
        Parent = stormXmlMember.TryGetMember(StormXmlConstants.ParentAttributeName, out StormXmlArray? parentProperty) ? parentProperty.Value : null;
        IsDefault = stormXmlMember.TryGetMember(StormXmlConstants.DefaultAttributeName, out StormXmlArray? defaultProperty) && defaultProperty.Value.HasValue && defaultProperty.Value == "1";

        HasId = !string.IsNullOrWhiteSpace(Id);
        HasParent = !string.IsNullOrWhiteSpace(Parent);
    }
}
