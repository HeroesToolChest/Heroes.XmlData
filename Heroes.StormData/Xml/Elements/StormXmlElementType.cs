namespace Heroes.StormData.Xml.Elements;

/// <summary>
/// Represents the type of a <see cref="StormXmlElement"/>.
/// </summary>
public enum StormXmlElementType
{
    /// <summary>
    /// Indicates that the type is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Indicates that the type is a constant.
    /// </summary>
    Const = 1,

    /// <summary>
    /// Indicates that the type is a class.
    /// </summary>
    Class = 2,

    /// <summary>
    /// Indicates that the type is a struct.
    /// </summary>
    Struct = 3,

    /// <summary>
    /// Indicates that the type is an enum.
    /// </summary>
    Enum = 4,

    /// <summary>
    /// Indicates that the type is a <c>Constant</c> element from a <c>.stormstyle</c> file.
    /// </summary>
    StyleConstant = 10,

    /// <summary>
    /// Indicates that the type is a <c>Style</c> element from a <c>.stormstyle</c> file.
    /// </summary>
    Style = 11,
}
