using Heroes.XmlData.CASC;

namespace Heroes.XmlData;

internal class CASCHeroesStorage : ICASCHeroesStorage
{
    public CASCHeroesStorage(CASCHandler cascHandler, CASCFolder cascFolderRoot)
    {
        CASCHandlerWrapper = new CASCHandlerWrapper(cascHandler);
        CASCFolderRoot = cascFolderRoot;
    }

    public ICASCHandlerWrapper CASCHandlerWrapper { get; }

    public CASCFolder CASCFolderRoot { get; }
}
