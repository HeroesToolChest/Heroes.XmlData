using System.Runtime.InteropServices;

namespace Heroes.StormData;

/// <summary>
/// Holds all the data across all the mods.
/// </summary>
internal class SourceContainer
{
    /// <summary>
    /// Gets the storage for the base, map, and custom data.
    /// </summary>
    public SplitStorage Storage { get; } = new();

    public void AddElement(StormXmlElement element, DataStorage dataStorage)
    {
        if (element.ElementType != StormXmlElementType.Class)
            return;

        string? elementType = GetElementTypeFromFileName(element);

        if (!string.IsNullOrWhiteSpace(elementType) && element.ElementName.AsSpan(1).StartsWith(elementType, StringComparison.OrdinalIgnoreCase))
        {
            AddBaseClassElementType(element, elementType, dataStorage);
        }
    }

    //private void AddElementWithNoId(string elementName, DataStorage dataStorage)
    //{
    //    ref StormXmlElement? stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(dataStorage.ElementCache.StormXmlElementByElementName, elementName, out bool exists);
    //}

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

    private static void AddBaseClassElementType(StormXmlElement element, string elementType, DataStorage dataStorage)
    {
        ref HashSet<string>? catalogNames = ref CollectionsMarshal.GetValueRefOrAddDefault(dataStorage.ElementCache.ClassElementsByCatalog, element.ElementName, out bool exists);

        if (!exists || catalogNames is null)
            catalogNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        catalogNames.Add(elementType);

        dataStorage.ElementCache.CatalogByClassElement.TryAdd(element.ElementName, elementType);
    }
}
