namespace Heroes.StormData;

/// <summary>
/// Contains the methods for a heroes storm data exception.
/// </summary>
public class HeroesStormDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesStormDataException"/> class.
    /// </summary>
    public HeroesStormDataException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesStormDataException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public HeroesStormDataException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesStormDataException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception.</param>
    public HeroesStormDataException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
