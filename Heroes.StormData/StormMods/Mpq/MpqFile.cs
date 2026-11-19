namespace Heroes.StormData.StormMods.Mpq;

internal sealed class MpqFile
{
    public MpqFile(string name, string fullName)
    {
        Name = name;
        FullName = fullName;
    }

    public string Name { get; }

    public string FullName { get; }
}
