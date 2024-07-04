
namespace Heroes.XmlData.CASC;

internal class CASCHandlerWrapper : ICASCHandlerWrapper
{
    private readonly CASCHandler _caschandler;

    public CASCHandlerWrapper(CASCHandler cascHandler)
    {
        _caschandler = cascHandler;
    }

    public bool FileExists(string file) => _caschandler.FileExists(file);

    public Stream OpenFile(string name) => _caschandler.OpenFile(name);
}
