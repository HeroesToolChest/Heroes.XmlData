namespace Heroes.XmlData.Tests.StormDepotCache;

[TestClass]
public class MapDependencyTests
{
    [TestMethod]
    public void GetMapDependencies_HasElements_ReturnsMapDependencies()
    {
        // arrange
        List<XElement> mapDependencies =
        [
            XElement.Parse(@"<Value>bnet:Volskaya Sound/0.0/1146,file:Mods\heroesmapmods/battlegroundmapmods/volskayasound.stormmod</Value>"),
            XElement.Parse(@"<Value>bnet:VolskayaSound/1.2/111,file:Mods\heroesmapmods/battlegroundmapmods/volskayasound.stormmod</Value>"),
        ];

        // act
        var results = MapDependency.GetMapDependencies(mapDependencies, "mods").ToList();

        // assert
        results.Should().SatisfyRespectively(
            first =>
            {
                first.BnetName.Should().Be("Volskaya Sound");
                first.BnetNamespace.Should().Be(1146);
                first.BnetVersionMajor.Should().Be(0);
                first.BnetVersionMinor.Should().Be(0);
                first.LocalFile.Should().Be("\\heroesmapmods\\battlegroundmapmods\\volskayasound.stormmod");
            },
            second =>
            {
                second.BnetName.Should().Be("VolskayaSound");
                second.BnetNamespace.Should().Be(111);
                second.BnetVersionMajor.Should().Be(1);
                second.BnetVersionMinor.Should().Be(2);
                second.LocalFile.Should().Be("\\heroesmapmods\\battlegroundmapmods\\volskayasound.stormmod");
            });
    }

    [TestMethod]
    public void GetMapDependencies_HasNoElements_ReturnsEmpty()
    {
        // arrange
        List<XElement> mapDependencies =
        [
        ];

        // act
        var results = MapDependency.GetMapDependencies(mapDependencies, "mods").ToList();

        // assert
        results.Should().BeEmpty();
    }
}
