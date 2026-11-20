using System.Runtime.InteropServices;

namespace Heroes.StormData.Cache;

/// <summary>
/// Represents a container for an individual data storage (e.g., base, map, or custom data).
/// </summary>
internal sealed class DataStorage
{
    public ElementCache ElementCache { get; } = new();

    public TextCache TextCache { get; } = new();

    public FileCache FileCache { get; } = new();

    public StyleCache StyleCache { get; } = new();

    /// <summary>
    /// Gets a number that changes whenever something that affects merged elements is added (a base class catalog name or an element).
    /// <para>
    /// Changes made directly to <see cref="ElementCache"/> don't change it.
    /// </para>
    /// </summary>
    public int ElementsVersion { get; private set; }

    public void AddConstElement(StormXmlConstElement element)
    {
        ElementCache.StormXmlConstElementById.Dictionary.TryAdd(element.Id, element);
    }

    /// <summary>
    /// Adds a <see cref="StormXmlStyleConstantElement"/>, or merges it into the existing one with the same <c>name</c>.
    /// </summary>
    /// <param name="element">The <see cref="StormXmlStyleConstantElement"/> to add.</param>
    public void AddStyleConstantElement(StormXmlStyleConstantElement element)
    {
        ref StormXmlStyleConstantElement? stormXmlStyleConstantElement = ref CollectionsMarshal.GetValueRefOrAddDefault(StyleCache.StormXmlStyleConstantElementByName.Dictionary, element.Name, out bool exists);

        if (!exists || stormXmlStyleConstantElement is null)
            stormXmlStyleConstantElement = element;
        else
            stormXmlStyleConstantElement.Merge(element);
    }

    /// <summary>
    /// Adds a <see cref="StormXmlStyleElement"/>, or merges it into the existing one with the same <c>name</c>.
    /// </summary>
    /// <param name="element">The <see cref="StormXmlStyleElement"/> to add.</param>
    public void AddStyleElement(StormXmlStyleElement element)
    {
        ref StormXmlStyleElement? stormXmlStyleElement = ref CollectionsMarshal.GetValueRefOrAddDefault(StyleCache.StormXmlStyleElementByName.Dictionary, element.Name, out bool exists);

        if (!exists || stormXmlStyleElement is null)
            stormXmlStyleElement = element;
        else
            stormXmlStyleElement.Merge(element);
    }

    public void AddBaseClassCatalogName(StormXmlElement element, string catalogName)
    {
        ref HashSet<string>? elementNames = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.ClassElementsByCatalog, catalogName, out bool exists);

        if (!exists || elementNames is null)
            elementNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        elementNames.Add(element.ElementName);

        ElementCache.CatalogByClassElement.Dictionary.TryAdd(element.ElementName, catalogName);

        ElementsVersion++;
    }

    public void AddElementWithNoId(StormXmlElement element)
    {
        ref StormXmlElement? stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.StormXmlElementByClassElement.Dictionary, element.ElementName, out bool exists);

        if (!exists || stormXmlElement is null)
            stormXmlElement = element;
        else
            stormXmlElement.Merge(element);

        ElementsVersion++;
    }

    public void AddElementWithId(StormXmlElement element, string catalogName)
    {
        ref var stormXmlElementById = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.StormXmlElementsByIdByCatalog.Dictionary, catalogName, out bool exists);

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
            ref var stormXmlElement = ref CollectionsMarshal.GetValueRefOrAddDefault(stormXmlElementById.Dictionary, element.Id!, out bool stormXmlElementExists);

            if (!stormXmlElementExists || stormXmlElement is null)
                stormXmlElement = element;
            else
                stormXmlElement.Merge(element);
        }

        ElementsVersion++;
    }

    public void AddLevelScalingElementValue(StormXmlArray levelScalingArray)
    {
        foreach (string index in levelScalingArray.GetIndexes())
        {
            if (!levelScalingArray.GetByIndex(index).TryGetMember("Modifications", out StormXmlArray? modificationsArray))
                continue;

            foreach (string modificationsIndex in modificationsArray.GetIndexes())
            {
                StormXmlMember modificationsElement = modificationsArray.GetByIndex(modificationsIndex);

                // skip if any member is missing or has no value attribute
                if (!modificationsElement.TryGetMember("Catalog", out StormXmlArray? catalog) || !catalog.Value.HasValue)
                    continue;
                if (!modificationsElement.TryGetMember("Entry", out StormXmlArray? entry) || !entry.Value.HasValue)
                    continue;
                if (!modificationsElement.TryGetMember("Field", out StormXmlArray? field) || !field.Value.HasValue)
                    continue;
                if (!modificationsElement.TryGetMember("Value", out StormXmlArray? value) || !value.Value.HasValue)
                    continue;

                ref var scalingFieldValueById = ref CollectionsMarshal.GetValueRefOrAddDefault(ElementCache.ScalingFieldValueByIdByCatalog.Dictionary, catalog.Value.RawValue, out bool exists);

                if (!exists)
                {
                    // case-sensitive for ids
                    scalingFieldValueById = new Dictionary<string, List<LevelScalingFieldValue>>(StringComparer.Ordinal)
                    {
                        { entry.Value.RawValue, new List<LevelScalingFieldValue> { new(field.Value.RawValue, value.Value.RawValue) } },
                    }
                    .GetAlternateLookup<ReadOnlySpan<char>>();
                }
                else
                {
                    ref var scalingFieldValues = ref CollectionsMarshal.GetValueRefOrAddDefault(scalingFieldValueById.Dictionary, entry.Value.RawValue, out bool scalingFieldValuesExists);

                    LevelScalingFieldValue scalingFieldValue = new(field.Value.RawValue, value.Value.RawValue);

                    if (!scalingFieldValuesExists || scalingFieldValues is null)
                        scalingFieldValues = [scalingFieldValue];
                    else
                        AddOrReplaceScalingFieldValue(scalingFieldValues, scalingFieldValue);
                }
            }
        }
    }

    /// <summary>
    /// Removes all the data from the <see cref="ElementCache"/>, <see cref="TextCache"/>, <see cref="FileCache"/>, and <see cref="StyleCache"/>.
    /// </summary>
    public void Clear()
    {
        ElementCache.Clear();
        TextCache.Clear();
        FileCache.Clear();
        StyleCache.Clear();

        // the elements were removed, so the merged elements are out of date.
        // the version only goes up (never reset), so the sum across the data storages can't land on an earlier value.
        ElementsVersion++;
    }

    /// <summary>
    /// Adds a <see cref="LevelScalingFieldValue"/> to a list, or replaces the one with the same <c>Field</c>.
    /// <para>The <c>Field</c> comparison is case-sensitive.</para>
    /// </summary>
    /// <param name="scalingFieldValues">The list to add to.</param>
    /// <param name="scalingFieldValue">The <see cref="LevelScalingFieldValue"/> to add, or to replace with.</param>
    internal static void AddOrReplaceScalingFieldValue(List<LevelScalingFieldValue> scalingFieldValues, LevelScalingFieldValue scalingFieldValue)
    {
        Span<LevelScalingFieldValue> scalingFieldValuesSpan = CollectionsMarshal.AsSpan(scalingFieldValues);

        for (int i = 0; i < scalingFieldValuesSpan.Length; i++)
        {
            if (string.Equals(scalingFieldValuesSpan[i].Field, scalingFieldValue.Field, StringComparison.Ordinal))
            {
                scalingFieldValuesSpan[i] = scalingFieldValue;

                return;
            }
        }

        scalingFieldValues.Add(scalingFieldValue);
    }
}
