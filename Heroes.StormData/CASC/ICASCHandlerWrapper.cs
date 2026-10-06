namespace Heroes.StormData.CASC;

internal interface ICASCHandlerWrapper
{
    bool FileExists(string file);

    Stream? OpenFile(string name);
}
