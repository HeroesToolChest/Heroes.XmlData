using BenchmarkDotNet.Attributes;
using Heroes.LocaleText;
using System.Xml.Linq;

namespace Heroes.XmlData.Benchmarks;

[MemoryDiagnoser]
public class XmlDataBenchmarks
{
    public XmlDataBenchmarks()
    {
    }

    [Benchmark]
    public TooltipDescription Test()
    {
        string description = "Increase the damage of Octo-Grab by <c val=\"#TooltipNumbers\"><d ref=\"Effect,OctoGrabPokeMasteryDamage,Amount * 100\"/>%</c>";

        HeroesXmlLoader loader = HeroesXmlLoader.LoadWithEmpty()
            .LoadCustomMod(new ManualModLoader("custom")
                .AddBaseElementTypes(new List<(string, string)>()
                {
                    ("Effect", "CEffectDamage"),
                })
                .AddElements(new List<XElement>()
                {
                    new(
                        "CEffectDamage",
                        new XAttribute("id", "OctoGrabPokeMasteryDamage"),
                        new XElement(
                            "Amount",
                            new XAttribute("value", "137"))),
                })
                .AddLevelScalingArrayElements(new List<XElement>()
                {
                    new(
                        "LevelScalingArray",
                        new XAttribute("Ability", "MurkyOctoGrab"),
                        new XElement(
                            "Modifications",
                            new XElement(
                                "Catalog",
                                new XAttribute("value", "Effect")),
                            new XElement(
                                "Entry",
                                new XAttribute("value", "OctoGrabPokeMasteryDamage")),
                            new XElement(
                                "Field",
                                new XAttribute("value", "Amount")),
                            new XElement(
                                "Value",
                                new XAttribute("value", "0.040000")))),
                }));

        HeroesData heroesData = loader.HeroesData;

        return heroesData.ParseGameString(description, StormLocale.ENUS);
    }
}