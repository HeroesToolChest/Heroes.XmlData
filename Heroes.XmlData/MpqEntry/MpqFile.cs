namespace Heroes.XmlData.MpqEntry;

internal class MpqFile
{
    public MpqFile(string fullName)
    {
        FullName = fullName;
    }

    public string Name => Path.GetFileName(FullName);

    public string FullName { get; }
}
