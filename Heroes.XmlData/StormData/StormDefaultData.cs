namespace Heroes.XmlData.StormData;

internal static class StormDefaultData
{
    public static List<(string DataObjectType, string ElementName)> DefaultBaseElementsTypes =>
        [
            ("Behavior", "CBehaviorBuff"),
        ];

    public static List<XElement> DefaultXElements =>
        [
            XElement.Parse(@"
<CBehaviorBuff>
  <Modification>
    <VitalMaxArray index=""Life"" />
    <VitalMaxArray index=""Shields"" />
    <VitalMaxArray index=""Energy"" />
  </Modification>
</CBehaviorBuff>
"),
        ];
}
