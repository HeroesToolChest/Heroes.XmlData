using CASCLib;

namespace Heroes.XmlData;

public class CASCHeroesStorage(CASCHandler cascHandler, CASCFolder cascFolderRoot)
{
    public CASCHandler CASCHandler { get; } = cascHandler;

    public CASCFolder CASCFolderRoot { get; } = cascFolderRoot;
}
