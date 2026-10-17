namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a <c>const</c> XML element.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormXmlConstElement : StormXmlElement
{
    internal StormXmlConstElement(StormXmlMember stormXmlMember, StormFile stormFile)
        : base(stormXmlMember, stormFile)
    {
    }

    private StormXmlConstElement(StormXmlConstElement other)
        : base(other)
    {
        EvaluateAsExpression = other.EvaluateAsExpression;
    }

    /// <inheritdoc/>
    public override string Id
    {
        get
        {
            if (base.Id is null)
                throw new HeroesStormDataException($"The 'id' attribute is required for const elements. Element '{ElementName}' in file '{StormFiles[0].StormModPath}' does not have an 'id' attribute.");

            return base.Id;
        }
    }

    /// <summary>
    /// Gets the value of the <c>value</c> attribute.
    /// <para>
    /// Any constant will have been evaluated and replaced with the actual value, unless the constant could not be evaluated.
    /// </para>
    /// </summary>
    public string Value => Members["value"].Value.RawValue ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the <c>evaluateAsExpression</c> attribute is set to <c>1</c>.
    /// </summary>
    public bool EvaluateAsExpression { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string parent = HasParent ? $", Parent = \"{Parent}\"" : string.Empty;

            return $"{{{ElementName}, Id = \"{Id}\"{parent}, IsEvaluateAsExpression = {EvaluateAsExpression}}}";
        }
    }

    /// <summary>
    /// Gets the value of the <c>value</c> attribute as a <see cref="double"/>.
    /// </summary>
    /// <returns>The value of the <c>value</c> attribute as a <see cref="double"/>.</returns>
    /// <exception cref="HeroesStormDataException">Thrown when the value cannot be converted to a <see cref="double"/>.</exception>
    public double GetValueAsDouble()
    {
        try
        {
            return double.Parse(Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new HeroesStormDataException($"Could not convert value '{Value}' to a double.", ex);
        }
    }

    /// <summary>
    /// Gets the value of the <c>value</c> attribute as an <see cref="int"/>.
    /// </summary>
    /// <returns>The value of the <c>value</c> attribute as an <see cref="int"/>.</returns>
    /// <exception cref="HeroesStormDataException">Thrown when the value cannot be converted to an <see cref="int"/>.</exception>
    public int GetValueAsInt32()
    {
        try
        {
            return int.Parse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new HeroesStormDataException($"Could not convert value '{Value}' to an int32.", ex);
        }
    }

    internal override StormXmlElement Clone() => new StormXmlConstElement(this);

    private protected override void Initialize(StormXmlMember stormXmlMember)
    {
        ElementName = stormXmlMember.Name;
        ElementNameWithDeclaration = ElementName;

        ElementType = StormXmlElementType.Const;
        EvaluateAsExpression = stormXmlMember.TryGetMember("evaluateAsExpression", out StormXmlArray? evaluateAsExpressionProperty) && evaluateAsExpressionProperty.Value == "1";
    }
}
