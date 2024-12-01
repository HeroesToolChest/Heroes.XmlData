using U8Xml;

namespace Heroes.XmlData.Tests.StormDepotCache;

[TestClass]
public class MapDependencyTests
{
    [TestMethod]
    public void GetMapDependencies_HasElements_ReturnsMapDependencies()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <DocInfo>
                <Dependencies>
                    <Value>bnet:Volskaya Sound/0.0/1146,file:Mods\heroesmapmods/battlegroundmapmods/volskayasound.stormmod</Value>
                    <Value>bnet:VolskayaSound/1.2/111,file:Mods\heroesmapmods/battlegroundmapmods/volskayasound.stormmod</Value>
                </Dependencies>
            </DocInfo>
            """);

        // act
        var results = MapDependency.GetMapDependencies(xmlObject.Root.Children.Find("Dependencies").Children, "mods").ToList();

        // assert
        results.Should().SatisfyRespectively(
            first =>
            {
                first.BnetName.Should().Be("Volskaya Sound");
                first.BnetNamespace.Should().Be(1146);
                first.BnetVersionMajor.Should().Be(0);
                first.BnetVersionMinor.Should().Be(0);
                first.LocalFile.Should().Be(Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
            },
            second =>
            {
                second.BnetName.Should().Be("VolskayaSound");
                second.BnetNamespace.Should().Be(111);
                second.BnetVersionMajor.Should().Be(1);
                second.BnetVersionMinor.Should().Be(2);
                second.LocalFile.Should().Be(Path.Join($"{Path.DirectorySeparatorChar}", "heroesmapmods", "battlegroundmapmods", "volskayasound.stormmod"));
            });
    }

    [TestMethod]
    public void GetMapDependencies_HasNoElements_ReturnsEmpty()
    {
        // arrange
        using XmlObject xmlObject = XmlParser.Parse(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <DocInfo>
                <Dependencies>
                </Dependencies>
            </DocInfo>
            """);

        // act
        var results = MapDependency.GetMapDependencies(xmlObject.Root.Children.Find("Dependencies").Children, "mods").ToList();

        // assert
        results.Should().BeEmpty();
    }
}
