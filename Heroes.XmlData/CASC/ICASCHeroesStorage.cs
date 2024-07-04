namespace Heroes.XmlData.CASC;

internal interface ICASCHeroesStorage
{
    CASCFolder CASCFolderRoot { get; }

    ICASCHandlerWrapper CASCHandlerWrapper { get; }
}