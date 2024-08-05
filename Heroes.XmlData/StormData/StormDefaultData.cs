namespace Heroes.XmlData.StormData;

internal static class StormDefaultData
{
    public static List<(string DataObjectType, string ElementName)> DefaultBaseElementsTypes =>
        [
            ("Behavior", "CBehaviorBuff"),
            ("Effect", "CEffectDamage"),
            ("Effect", "CEffectCreateHealer"),
            ("Unit", "CUnit"),
        ];

    public static List<XElement> DefaultXElements =>
        [
            XElement.Parse(
"""
<CBehaviorBuff>
  <DamageResponse ModifyLimit="0" />
  <Modification>
    <DamageDealtScaled index="Basic" value="0" />
    <DamageDealtScaled index="Ability" value="0" />
    <DamageDealtFraction index="Basic" value="0" />
    <DamageDealtFraction index="Ability" value="0" />
    <VitalMaxArray index="Life" value="0" />
    <VitalMaxArray index="Shields" value="0" />
    <VitalMaxArray index="Energy" value="0" />
    <VitalRegenArray index="Life" value="0" />
    <VitalRegenArray index="Shields" value="0" />
    <VitalRegenArray index="Energy" value="0" />
  </Modification>
</CBehaviorBuff>
"""),
            XElement.Parse("""
<CEffectDamage>
  <Amount value="0" /> 
</CEffectDamage>
"""),
            XElement.Parse("""
<CEffectCreateHealer>
  <RechargeVitalRate value="0" />
</CEffectCreateHealer>
"""),
            XElement.Parse("""
<CUnit>
    <ShieldRegenRate value="0" />
</CUnit>
"""),
        ];
}
