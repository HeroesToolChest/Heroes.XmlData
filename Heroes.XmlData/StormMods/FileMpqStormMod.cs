namespace Heroes.XmlData.StormMods;

internal class FileMpqStormMod : MpqStormMod<FileHeroesSource>
{
    private readonly string _directoryPath;
    private readonly string _name;

    public FileMpqStormMod(FileHeroesSource heroesSource, string directoryPath, string name)
        : base(heroesSource)
    {
        _directoryPath = directoryPath;
        _name = name;
    }

    public override string DirectoryPath => _directoryPath;

    public override string Name => _name is null ? base.Name : _name;

    protected override string MpqDirectoryPath => Path.Join(HeroesSource.ModsDirectoryPath, _directoryPath);

    protected override void AddXmlFile(string xmlFilePath)
    {
        if (!ValidateXmlFile(xmlFilePath, out XDocument? document))
            return;

        XmlStorage.AddXmlFile(document, xmlFilePath);
    }

    protected override Stream GetMpqFile(string file) => File.OpenRead(file);

    protected override IStormMod GetStormMod(string path) => HeroesSource.CreateStormModInstance<FileStormMod>(HeroesSource, path);

    protected override bool TryGetFile(string filePath, [NotNullWhen(true)] out Stream? stream)
    {
        throw new NotImplementedException();
    }
}
