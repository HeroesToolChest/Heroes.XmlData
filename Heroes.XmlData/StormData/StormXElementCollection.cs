using System.Diagnostics;

namespace Heroes.XmlData.StormData;

[DebuggerDisplay("Count = {ElementsById.Count}")]
internal class StormXElementCollection
{
    public Dictionary<StormElementId, List<StormXElementValuePath>> ElementsById { get; } = [];
}
