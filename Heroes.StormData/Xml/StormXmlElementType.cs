namespace Heroes.StormData.Xml;

/// <summary>
/// Represents the type of a <see cref="StormXmlElement"/>.
/// </summary>
public enum StormXmlElementType
{
    /// <summary>
    /// Indicates that the type is unknown.
    /// </summary>
    Unknown = -2,

    /// <summary>
    /// Indicates that the type is a constant.
    /// </summary>
    Const = -1,

    /// <summary>
    /// Indicates that the type is a class.
    /// </summary>
    Class = 0,

    /// <summary>
    /// Indicates that the type is a struct.
    /// </summary>
    Struct = 1,

    /// <summary>
    /// Indicates that the type is an enum.
    /// </summary>
    Enum = 2,
}
