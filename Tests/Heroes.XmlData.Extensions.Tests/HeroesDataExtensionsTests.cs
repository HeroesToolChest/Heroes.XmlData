//namespace Heroes.XmlData.Extensions.Tests;

//[TestClass]
//public class HeroesDataExtensionsTests
//{
//    [TestMethod]
//    public void GetValueFromConstElement_WithConstant_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
//            .AddConstantElements(new List<XElement>()
//            {
//                new(
//                    "const",
//                    new XAttribute("id", "$GazloweGravOBomb3000Damage"),
//                    new XAttribute("value", "220")),
//            });

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromConstElement(new XElement(
//            "const",
//            new XAttribute("id", "$GazloweGravOBomb3000MiniatureBlackholeVoidZoneDamage"),
//            new XAttribute("value", "/($GazloweGravOBomb3000Damage 10)"),
//            new XAttribute("evaluateAsExpression", "1")));

//        // assert
//        value.Should().Be(22);
//    }

//    [TestMethod]
//    public void GetValueFromConstElement_WithNoConstant_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromConstElement(new XElement(
//            "const",
//            new XAttribute("id", "$GazloweXplodiumChargeRange"),
//            new XAttribute("value", "8")));

//        // assert
//        value.Should().Be(8);
//    }

//    [TestMethod]
//    public void GetValueFromConstElement_WithEvalIsZero_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromConstElement(new XElement(
//            "const",
//            new XAttribute("id", "$GazloweXplodiumChargeRange"),
//            new XAttribute("value", "8"),
//            new XAttribute("evaluateAsExpression", "0")));

//        // assert
//        value.Should().Be(8);
//    }

//    [TestMethod]
//    public void GetValueFromConstElement_WithNoValueAttribute_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromConstElement(new XElement(
//            "const",
//            new XAttribute("id", "$GazloweXplodiumChargeRange"),
//            new XAttribute("evaluateAsExpression", "1")));

//        // assert
//        value.Should().Be(0);
//    }

//    [TestMethod]
//    public void GetValueFromElement_ElementHasValueAttributeWithConstant_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
//            .AddConstantElements(new List<XElement>()
//            {
//                new(
//                    "const",
//                    new XAttribute("id", "$GazloweHeroWeaponDamage"),
//                    new XAttribute("value", "100")),
//            });

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromElement(new XElement(
//            "Amount",
//            new XAttribute("value", "$GazloweHeroWeaponDamage")));

//        // assert
//        value.Should().Be(100);
//    }

//    [TestMethod]
//    public void GetValueFromElement_ElementHasValueAttributeWithNumber_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromElement(new XElement(
//            "Vital",
//            new XAttribute("index", "Energy"),
//            new XAttribute("Value", " 50 ")));

//        // assert
//        value.Should().Be(50);
//    }

//    [TestMethod]
//    public void GetValueFromElement_ElementDoesNotHaveValueAttribute_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromElement(new XElement(
//            "Vital",
//            new XAttribute("index", "Energy")));

//        // assert
//        value.Should().Be(0);
//    }

//    [TestMethod]
//    public void GetValueFromValueText_ValueTextIsNumber_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromValueText("50");
//        double valueAsSpan = heroesData.GetValueFromValueText("50".AsSpan());

//        // assert
//        value.Should().Be(50);
//        valueAsSpan.Should().Be(50);
//    }

//    [TestMethod]
//    public void GetValueFromValueText_ValueTextIsText_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromValueText("CasterUnit");
//        double valueAsSpan = heroesData.GetValueFromValueText("CasterUnit".AsSpan());

//        // assert
//        value.Should().Be(0);
//        valueAsSpan.Should().Be(0);
//    }

//    [TestMethod]
//    public void GetValueFromValueText_ValueTextIsEmpty_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty();

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromValueText(string.Empty);
//        double valueAsSpan = heroesData.GetValueFromValueText(string.Empty.AsSpan());

//        // assert
//        value.Should().Be(0);
//        valueAsSpan.Should().Be(0);
//    }

//    [TestMethod]
//    public void GetValueFromValueText_ValueTextIsConstant_ReturnsValue()
//    {
//        // arrange
//        HeroesXmlLoader loader = HeroesXmlLoader.LoadAsEmpty()
//            .AddConstantElements(new List<XElement>()
//            {
//                new(
//                    "const",
//                    new XAttribute("id", "$GazloweHeroWeaponDamage"),
//                    new XAttribute("value", "100")),
//            });

//        HeroesData heroesData = loader.HeroesData;

//        // act
//        double value = heroesData.GetValueFromValueText("$GazloweHeroWeaponDamage");
//        double valueAsSpan = heroesData.GetValueFromValueText("$GazloweHeroWeaponDamage".AsSpan());

//        // assert
//        value.Should().Be(100);
//        valueAsSpan.Should().Be(100);
//    }
//}

