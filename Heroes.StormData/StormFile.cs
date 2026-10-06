namespace Heroes.StormData;

public class StormFile
{
    internal StormFile()
    {
    }

    /// <summary>
    /// Gets the name of the <c>stormmod</c> that this file exists in.
    /// </summary>
    public required string StormModName { get; init; }

    /// <summary>
    /// Gets the relative path to the <c>stormmod</c> that this file exists in.
    /// </summary>
    public required string StormModPath { get; init; }


}
