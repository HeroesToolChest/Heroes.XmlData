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

    //public static Dictionary<string, Dictionary<string, string>> Data => new(StringComparer.OrdinalIgnoreCase)
    //{
    //    {
    //        "Behavior", new(StringComparer.OrdinalIgnoreCase)
    //        {
    //            { "Modification.VitalMaxArray[0]", "Modification.VitalMaxArray[Life]" },
    //            { "Modification.VitalMaxArray[1]", "Modification.VitalMaxArray[Shields]" },
    //            { "Modification.VitalMaxArray[2]", "Modification.VitalMaxArray[Energy]" },
    //        }
    //    },
    //};
}
