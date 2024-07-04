namespace Heroes.XmlData.CASC;

internal interface ICASCHandlerWrapper
{
    bool FileExists(string file);

    Stream OpenFile(string name);
}
