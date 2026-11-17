namespace Heroes.XmlData;

internal sealed class HeroesLoggerOptions : ILoggerOptions
{
    public string LogFileName => "casclib.log";

    public bool TimeStamp => true;
}
