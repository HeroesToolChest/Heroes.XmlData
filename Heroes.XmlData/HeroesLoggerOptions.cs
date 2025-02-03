namespace Heroes.XmlData;

internal class HeroesLoggerOptions : ILoggerOptions
{
    public string LogFileName => "casclib.log";

    public bool TimeStamp => true;
}
