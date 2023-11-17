using System.ComponentModel;

namespace Heroes.XmlData;

/// <summary>
/// Specifies the localization.
/// </summary>
public enum HeroesLocalization
{
    /// <summary>
    /// Indicates English locale.
    /// </summary>
    [Description("enus.stormdata")]
    ENUS,

    /// <summary>
    /// Indicates German locale.
    /// </summary>
    [Description("dede.stormdata")]
    DEDE,

    /// <summary>
    /// Indicates Spanish (EU) locale.
    /// </summary>
    [Description("eses.stormdata")]
    ESES,

    /// <summary>
    /// Indicates Spanish (AL) locale.
    /// </summary>
    [Description("esmx.stormdata")]
    ESMX,

    /// <summary>
    /// Indicates French locale.
    /// </summary>
    [Description("frfr.stormdata")]
    FRFR,

    /// <summary>
    /// Indicates Italian locale.
    /// </summary>
    [Description("itit.stormdata")]
    ITIT,

    /// <summary>
    /// Indicates Korean locale.
    /// </summary>
    [Description("kokr.stormdata")]
    KOKR,

    /// <summary>
    /// Indicates Polish locale.
    /// </summary>
    [Description("plpl.stormdata")]
    PLPL,

    /// <summary>
    /// Indicates Portuguese locale.
    /// </summary>
    [Description("ptbr.stormdata")]
    PTBR,

    /// <summary>
    /// Indicates Russian locale.
    /// </summary>
    [Description("ruru.stormdata")]
    RURU,

    /// <summary>
    /// Indicates Chinese locale.
    /// </summary>
    [Description("zhcn.stormdata")]
    ZHCN,

    /// <summary>
    /// Indicates Chinese (TW) locale.
    /// </summary>
    [Description("zhtw.stormdata")]
    ZHTW,
}
