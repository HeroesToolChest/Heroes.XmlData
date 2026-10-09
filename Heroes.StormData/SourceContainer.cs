using System.Runtime.InteropServices;

namespace Heroes.StormData;

/// <summary>
/// Holds the data across all the <c>stormmods</c>.
/// </summary>
internal class SourceContainer
{
    /// <summary>
    /// Gets the storage for the base, map, and custom data.
    /// </summary>
    public LayeredDataStorage LayeredDataStorage { get; } = new();

    //public void AddElement(StormXmlElement element, DataStorage dataStorage)
    //{
    //    if (element.ElementType != StormXmlElementType.Class)
    //        return;

    //    string? elementType = GetElementTypeFromFileName(element);

    //    if (!string.IsNullOrWhiteSpace(elementType) && element.ElementName.AsSpan(1).StartsWith(elementType, StringComparison.OrdinalIgnoreCase))
    //    {
    //        AddBaseClassElementType(element, elementType, dataStorage);
    //    }

    //    if (string.IsNullOrEmpty(element.Id))
    //    {
    //        AddElementWithNoId(element, dataStorage);
    //        return;
    //    }


    //}

    //public static void AddConstElement(StormXmlConstElement element, DataStorage dataStorage)
    //{
    //    if (element.ElementType != StormXmlElementType.Const)
    //        return;

    //    dataStorage.ElementCache.StormXmlConstElementById.TryAdd(element.Id, element);
    //}


    //private static string? GetElementTypeFromFileName(StormXmlElement element)
    //{
    //    string fileName = Path.GetFileName(element.StormFiles[0].StormModPath);

    //    int index = fileName.LastIndexOf("data.xml", StringComparison.OrdinalIgnoreCase);
    //    if (index > 1)
    //    {
    //        return fileName[..index];
    //    }

    //    return null;
    //}

    //private static void AddBaseClassElementType(StormXmlElement element, string elementType, DataStorage dataStorage)
    //{
    //    ref HashSet<string>? catalogNames = ref CollectionsMarshal.GetValueRefOrAddDefault(dataStorage.ElementCache.ClassElementsByCatalog, element.ElementName, out bool exists);

    //    if (!exists || catalogNames is null)
    //        catalogNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    //    catalogNames.Add(elementType);

    //    dataStorage.ElementCache.CatalogByClassElement.TryAdd(element.ElementName, elementType);
    //}

    //private static void AddElementWithNoId(StormXmlElement element, DataStorage dataStorage)
    //{
    //    ref StormXmlElement? stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(dataStorage.ElementCache.StormXmlElementByClassElement, element.ElementName, out bool exists);

    //    if (!exists || stormXmlElement is null)
    //        stormXmlElement = element;
    //    else
    //        stormXmlElement.Merge(element);
    //}
}
