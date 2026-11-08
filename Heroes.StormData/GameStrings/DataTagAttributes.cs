namespace Heroes.StormData.GameStrings;

/// <summary>
/// The attributes of a data tag that give its value, e.g. <c>&lt;d ref="Effect,GuldanHorrifyDamage,Amount" precision="2"/&gt;</c>.
/// </summary>
internal readonly ref struct DataTagAttributes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTagAttributes"/> struct.
    /// </summary>
    /// <param name="hasConst">Whether the tag has a <c>const</c> attribute.</param>
    /// <param name="constId">The value of the <c>const</c> attribute.</param>
    /// <param name="hasRef">Whether the tag has a <c>ref</c> attribute.</param>
    /// <param name="dRef">The value of the <c>ref</c> attribute.</param>
    /// <param name="precision">The value of the <c>precision</c> attribute, or 0 if it doesn't have one.</param>
    public DataTagAttributes(bool hasConst, ReadOnlySpan<char> constId, bool hasRef, ReadOnlySpan<char> dRef, int precision)
    {
        HasConst = hasConst;
        ConstId = constId;
        HasRef = hasRef;
        Ref = dRef;
        Precision = precision;
    }

    /// <summary>
    /// Gets a value indicating whether the tag has a <c>const</c> attribute. It is used over the <c>ref</c> attribute when the tag has both.
    /// </summary>
    public bool HasConst { get; }

    /// <summary>
    /// Gets the value of the <c>const</c> attribute, e.g. <c>$YrelSacredGroundArmorBonus</c>.
    /// </summary>
    public ReadOnlySpan<char> ConstId { get; }

    /// <summary>
    /// Gets a value indicating whether the tag has a <c>ref</c> attribute.
    /// </summary>
    public bool HasRef { get; }

    /// <summary>
    /// Gets the value of the <c>ref</c> attribute, e.g. <c>Effect,GuldanHorrifyDamage,Amount</c>.
    /// </summary>
    public ReadOnlySpan<char> Ref { get; }

    /// <summary>
    /// Gets the number of decimal places the value is rounded to, or 0 if the tag doesn't have a <c>precision</c> attribute.
    /// </summary>
    public int Precision { get; }

    /// <summary>
    /// Creates the attributes of a tag with a <c>const</c> attribute.
    /// </summary>
    /// <param name="constId">The value of the <c>const</c> attribute.</param>
    /// <param name="precision">The value of the <c>precision</c> attribute.</param>
    /// <returns>The <see cref="DataTagAttributes"/>.</returns>
    public static DataTagAttributes FromConst(ReadOnlySpan<char> constId, int precision) => new(true, constId, false, default, precision);

    /// <summary>
    /// Creates the attributes of a tag with a <c>ref</c> attribute.
    /// </summary>
    /// <param name="dRef">The value of the <c>ref</c> attribute.</param>
    /// <param name="precision">The value of the <c>precision</c> attribute.</param>
    /// <returns>The <see cref="DataTagAttributes"/>.</returns>
    public static DataTagAttributes FromRef(ReadOnlySpan<char> dRef, int precision) => new(false, default, true, dRef, precision);
}
