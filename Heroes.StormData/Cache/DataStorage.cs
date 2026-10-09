using System.Runtime.InteropServices;

namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a container for an individual data storage (e.g., base, map, or custom data).
/// </summary>
internal class DataStorage
{
    public ElementCache ElementCache { get; } = new();

    public void AddElement(StormXmlElement element)
    {
        if (element.ElementType != StormXmlElementType.Class)
            return;

        string? elementType = GetElementTypeFromFileName(element);

        if (!string.IsNullOrWhiteSpace(elementType) && element.ElementName.AsSpan(1).StartsWith(elementType, StringComparison.OrdinalIgnoreCase))
        {
            AddBaseClassElementType(element, elementType);
        }

        if (string.IsNullOrEmpty(element.Id))
        {
            AddElementWithNoId(element);
            return;
        }
    }

    public void AddConstElement(StormXmlConstElement element)
    {
        if (element.ElementType != StormXmlElementType.Const)
            return;

        ElementCache.StormXmlConstElementById.TryAdd(element.Id, element);
    }

    private static string? GetElementTypeFromFileName(StormXmlElement element)
    {
        string fileName = Path.GetFileName(element.StormFiles[0].StormModPath);

        int index = fileName.LastIndexOf("data.xml", StringComparison.OrdinalIgnoreCase);
        if (index > 1)
        {
            return fileName[..index];
        }

        return null;
    }

    private void AddBaseClassElementType(StormXmlElement element, string elementType)
    {
        ref HashSet<string>? catalogNames = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.ClassElementsByCatalog, element.ElementName, out bool exists);

        if (!exists || catalogNames is null)
            catalogNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        catalogNames.Add(elementType);

        ElementCache.CatalogByClassElement.TryAdd(element.ElementName, elementType);
    }

    private void AddElementWithNoId(StormXmlElement element)
    {
        ref StormXmlElement? stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.StormXmlElementByClassElement, element.ElementName, out bool exists);

        if (!exists || stormXmlElement is null)
            stormXmlElement = element;
        else
            stormXmlElement.Merge(element);
    }
}
