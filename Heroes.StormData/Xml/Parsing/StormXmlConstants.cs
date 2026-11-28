using System.Collections.Frozen;

namespace Heroes.StormData.Xml.Parsing;

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
    public const string UnitNameAttributeName = "unitName";
    public const string TokenDelimiter = "##";

    // ability links
    public const string AbilElementNamePrefix = "CAbil";
    public const string AbilReferencePrefix = "Abil,";
    public const string LinkFieldName = "Link";
    public const string ReferenceFieldName = "Reference";
    public const string AbilFieldName = "Abil";
    public const string AbilArrayFieldName = "AbilArray";
    public const string CountFieldName = "Count";
    public const string RequirementCountAbilElementName = "CRequirementCountAbil";
    public const string RequirementAllowAbilElementName = "CRequirementAllowAbil";
    public const string SubmenuAbilStateFieldName = "SubmenuAbilState";

    // .stormstyle files
    public const string StyleConstantElementName = "Constant";
    public const string StyleElementName = "Style";
    public const string NameAttributeName = "name";
    public const string ValAttributeName = "val";
    public const string TemplateAttributeName = "template";

    // elements that are arrays that don't have the "array" suffix in their name
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

    // the fields that are an ability link (or an ability command, which starts with the ability link) in every element
    public static readonly FrozenSet<string> AbilLinkFieldNames = new HashSet<string>(StringComparer.Ordinal)
    {
        AbilFieldName,
        "AbilCmd",
        "AbilCommandArray",
        "Ability",
        "AbilLink",
        "AbilLinkDisableArray",
        "AbilLinkEnableArray",
        "BuildMorphAbil",
        "CaptureAbility",
        "CooldownDisplay",
        "FinishCommand",
        "IdleCommand",
        "MagazineAbil",
        "ModifyAbil",
        "ParentAbil",
        "QueryAbilCmd",
        "SelectAbilCmd",
        SubmenuAbilStateFieldName,
        "TauntAbil",
        "UnlinkMorphAbil",
    }
    .ToFrozenSet(StringComparer.Ordinal);

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
