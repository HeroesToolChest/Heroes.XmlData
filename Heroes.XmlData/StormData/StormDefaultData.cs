namespace Heroes.XmlData.StormData;

internal static class StormDefaultData
{
    public static List<(string DataObjectType, string ElementName)> DefaultBaseElementsTypes =>
        [
            ("Behavior", "CBehaviorBuff"),
            ("Effect", "CEffectDamage"),
            ("Effect", "CEffectCreateHealer"),
            ("Unit", "CUnit"),
            ("ScoreValue", "CScoreValueCustom"),
        ];

    public static List<XElement> DefaultXElements =>
        [
            XElement.Parse(
"""
<CBehaviorBuff>
  <DamageResponse ModifyLimit="0" ModifyFraction="1" />
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
            XElement.Parse("""
<CScoreValueCustom default="1" />
"""),
        ];
}
