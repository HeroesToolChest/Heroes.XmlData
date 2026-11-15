namespace Heroes.StormData;

/// <summary>
/// Represents a file from a <c>stormmod</c>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class StormFile
{
    internal StormFile()
    {
    }

    /// <summary>
    /// Gets the name of the <c>stormmod</c> that this file exists in (e.g. <c>core</c>).
    /// </summary>
    public required string StormModName { get; init; }

    /// <summary>
    /// Gets the path of the file relative to the <c>mods</c> directory, which starts with the <c>stormmod</c> directory
    /// (e.g. <c>core.stormmod\base.stormdata\gamedata.xml</c>).
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the full path of the file (e.g. <c>C:\heroesbuild\mods\core.stormmod\base.stormdata\gamedata.xml</c>).
    /// </summary>
    public required string FullPath { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{{StormModName = \"{StormModName}\", RelativePath = \"{RelativePath}\"}}";
}
