﻿using Heroes.XmlData.StormData;

namespace Heroes.XmlData.Tests.StormData;

[TestClass]
public class StormXElementIdTests
{
    [TestMethod]
    [DataRow("CEffectDamage", "effect1", "CEffectDamage", "effect1")]
    [DataRow("CEffectDamage", "Effect1", "CEffectDamage", "effect1")]
    [DataRow("CEffeCtDamage", "Effect1", "cEffectDamage", "effEct1")]
    public void Equals_AreEquals_ReturnsTrue(string elementName1, string id1, string elementName2, string id2)
    {
        // arrange
        StormElementId stormXElementId1 = new(elementName1, id1);
        StormElementId stormXElementId2 = new(elementName2, id2);

        // act
        bool result = stormXElementId1.Equals(stormXElementId2);

        // assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("CEffectDamage", "effect1", "CEffectDamage", "effect2")]
    [DataRow("CEffectDamage", "Effect2", "CEffectDamage", "effect1")]
    [DataRow("CEffeCtDamage", "Effect1", "cEffectDamage1", "effEct1")]
    public void Equals_AreNotEquals_ReturnsFalse(string elementName1, string id1, string elementName2, string id2)
    {
        // arrange
        StormElementId stormXElementId1 = new(elementName1, id1);
        StormElementId stormXElementId2 = new(elementName2, id2);

        // act
        bool result = stormXElementId1.Equals(stormXElementId2);

        // assert
        result.Should().BeFalse();
    }
}
