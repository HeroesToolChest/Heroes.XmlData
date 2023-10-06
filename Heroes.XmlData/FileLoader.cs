namespace Heroes.XmlData;

public class FileLoader
{
    private FileLoader()
    {
    }

    public static FileLoader GetInstance()
    {
        return new FileLoader();
    }
}
