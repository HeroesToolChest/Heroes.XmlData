namespace Heroes.StormData;

internal class HeroesLoggerOptions : ILoggerOptions
{
    public string LogFileName => "casclib.log";

    public bool TimeStamp => true;
}
