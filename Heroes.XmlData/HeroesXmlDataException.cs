namespace Heroes.XmlData;

/// <summary>
/// Contains the methods for a heroes xml data exception.
/// </summary>
public class HeroesXmlDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesXmlDataException"/> class.
    /// </summary>
    public HeroesXmlDataException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesXmlDataException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public HeroesXmlDataException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesXmlDataException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception.</param>
    public HeroesXmlDataException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
