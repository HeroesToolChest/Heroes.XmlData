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
    public const string TokenInstructionName = "token";
    public const string TokenTypeAttributeName = "type";

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

    // elements that are arrays only inside a specific root element type, e.g. Cost is an array in CEffectModifyUnit but not in CAbil elements
    public static readonly FrozenDictionary<string, string> ElementArraysByRootElementType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Cost", "CEffectModifyUnit" },
        { "String", "CUser" },
        { "Text", "CUser" },
    }
    .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // attribute names that StormXmlMember reads back from its members; a token with one of these ids would overwrite the real attribute
    public static readonly FrozenSet<string> ReservedTokenIds = new HashSet<string>(StringComparer.Ordinal)
    {
        IdAttributeName,
        ParentAttributeName,
        DefaultAttributeName,
        ValueAttributeName,
    }
    .ToFrozenSet();
}
