using System.Collections.Frozen;

namespace Heroes.StormData.Xml;

internal sealed class StormXmlConstants
{
    public const string IdAttributeName = "id";
    public const string ParentAttributeName = "parent";
    public const string DefaultAttributeName = "default";
    public const string IndexAttributeName = "index";
    public const string ValueAttributeName = "value";
    public const string ConstElementName = "const";

    // elements that are array that don't have the "array" suffix in their name
    public static readonly FrozenSet<string> ElementArrays = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Buttons",
        "CardLayouts",
        "CatalogModifications",
        "ConditionalEvents",
        "DurationOverride",
        "Flags",
        "Instances",
        "LayoutButtons",
        "Modifications",
        "On",
        "Remove",
        "RolesMultiClass",
        "TooltipAppender",
    }
    .ToFrozenSet();
}
