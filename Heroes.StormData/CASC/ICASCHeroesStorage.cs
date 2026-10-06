namespace Heroes.StormData.CASC;

internal interface ICASCHeroesStorage
{
    CASCFolder CASCFolderRoot { get; }

    ICASCHandlerWrapper CASCHandlerWrapper { get; }
}