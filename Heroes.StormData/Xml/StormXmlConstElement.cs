namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a <c>const</c> XML element.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormXmlConstElement : StormXmlElement
{
    internal StormXmlConstElement(StormXmlMember stormXmlProperty, StormFile stormFile)
        : base(stormXmlProperty, stormFile)
    {
    }

    /// <inheritdoc/>
    public override string Id => base.Id!;

    /// <summary>
    /// Gets the raw value of the <c>value</c> attribute.
    /// </summary>
    public string RawValue => Members["value"].RawValue ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the <c>evaluateAsExpression</c> attribute is set to <c>1</c>.
    /// </summary>
    public bool EvaluateAsExpression { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string parent = string.IsNullOrWhiteSpace(Parent) ? string.Empty : $", Parent = \"{Parent}\"";

            return $"{{{ElementName}, Id = \"{Id}\"{parent}, IsEvaluateAsExpression = {EvaluateAsExpression}}}";
        }
    }

    private protected override void Initialize(StormXmlMember stormXmlProperty)
    {
        ElementName = stormXmlProperty.Name;
        ElementType = StormXmlElementType.Const;
        EvaluateAsExpression = stormXmlProperty.TryGetMember("evaluateAsExpression", out StormXmlArray? evaluateAsExpressionProperty) && evaluateAsExpressionProperty.Value == "1";
    }
}
