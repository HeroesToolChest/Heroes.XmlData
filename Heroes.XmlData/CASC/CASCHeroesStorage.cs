namespace Heroes.XmlData.CASC;

internal sealed class CASCHeroesStorage : ICASCHeroesStorage
{
    public CASCHeroesStorage(CASCHandler cascHandler, CASCFolder cascFolderRoot)
    {
        CASCHandlerWrapper = new CASCHandlerWrapper(cascHandler);
        CASCFolderRoot = cascFolderRoot;
    }

    public ICASCHandlerWrapper CASCHandlerWrapper { get; }

    public CASCFolder CASCFolderRoot { get; }
}
