using System.Runtime.InteropServices;

namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a container for an individual data storage (e.g., base, map, or custom data).
/// </summary>
internal sealed class DataStorage
{
    public ElementCache ElementCache { get; } = new();

    public void AddConstElement(StormXmlConstElement element)
    {
        ElementCache.StormXmlConstElementById.TryAdd(element.Id, element);
    }

    public void AddBaseClassCatalogName(StormXmlElement element, string catalogName)
    {
        ref HashSet<string>? elementNames = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.ClassElementsByCatalog, catalogName, out bool exists);

        if (!exists || elementNames is null)
            elementNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        elementNames.Add(element.ElementName);

        ElementCache.CatalogByClassElement.TryAdd(element.ElementName, catalogName);
    }

    public void AddElementWithNoId(StormXmlElement element)
    {
        ref StormXmlElement? stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.StormXmlElementByClassElement, element.ElementName, out bool exists);

        if (!exists || stormXmlElement is null)
            stormXmlElement = element;
        else
            stormXmlElement.Merge(element);
    }

    public void AddElementWithId(StormXmlElement element, string catalogName)
    {
        ref var stormXmlElementById = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.StormXmlElementsByCatalog, catalogName, out bool exists);

        if (!exists)
        {
            // case-sensitive for ids
            stormXmlElementById = new Dictionary<string, StormXmlElement>(StringComparer.Ordinal)
            {
                { element.Id!, element },
            }
            .GetAlternateLookup<ReadOnlySpan<char>>();
        }
        else
        {
            ref var stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(stormXmlElementById, element.Id, out bool stormXmlElementExists);

            if (!stormXmlElementExists || stormXmlElement is null)
                stormXmlElement = element;
            else
                stormXmlElement.Merge(element);
        }
    }
}
