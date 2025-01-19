namespace Heroes.XmlData;

internal class HeroesLoggerOptions : ILoggerOptions
{
    public string LogFileName => Path.Join("logs", "casclib.log");

    public bool TimeStamp => true;
}
