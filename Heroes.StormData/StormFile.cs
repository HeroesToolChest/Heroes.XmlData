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
    /// <para>
    /// If the file is inside an MPQ archive (see <see cref="IsInMpqArchive"/>), this is the archive's path followed by the path of the file inside the archive,
    /// which cannot be opened directly. Use <see cref="MpqFilePath"/> and <see cref="MpqEntryPath"/> instead.
    /// </para>
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>
    /// Gets the full path of the MPQ archive (e.g. a <c>.s2ma</c> or <c>.stormmap</c> file) that contains this file,
    /// or <see langword="null"/> if the file is not inside an MPQ archive.
    /// </summary>
    public string? MpqFilePath { get; init; }

    /// <summary>
    /// Gets the path of this file inside the MPQ archive at <see cref="MpqFilePath"/> (e.g. <c>base.stormdata\gamedata\actordata.xml</c>),
    /// or <see langword="null"/> if the file is not inside an MPQ archive.
    /// </summary>
    public string? MpqEntryPath { get; init; }

    /// <summary>
    /// Gets a value indicating whether this file is inside an MPQ archive.
    /// </summary>
    [MemberNotNullWhen(true, nameof(MpqFilePath), nameof(MpqEntryPath))]
    public bool IsInMpqArchive => MpqFilePath is not null && MpqEntryPath is not null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{{StormModName = \"{StormModName}\", RelativePath = \"{RelativePath}\"}}";
}
