namespace Heroes.XmlData;

/// <summary>
/// The class for a casc heroes storage.
/// </summary>
/// <param name="cascHandler">The handler for the storage.</param>
/// <param name="cascFolderRoot">The root folder of the casc storage.</param>
public class CASCHeroesStorage(CASCHandler cascHandler, CASCFolder cascFolderRoot)
{
    /// <summary>
    /// Gets the <see cref="CASCHandler"/>.
    /// </summary>
    public CASCHandler CASCHandler { get; } = cascHandler;

    /// <summary>
    /// Gets the <see cref="CASCFolder"/>.
    /// </summary>
    public CASCFolder CASCFolderRoot { get; } = cascFolderRoot;
}
