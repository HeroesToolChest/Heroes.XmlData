﻿using System.Diagnostics;

namespace Heroes.XmlData.StormData;

/// <summary>
/// Contains the data for a storm style style <see cref="XElement"/>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormStyleStyleElement : StormElement
{
    private const string NameAttribute = "name";
    private const string TemplateAttribute = "template";

    /// <summary>
    /// Initializes a new instance of the <see cref="StormStyleStyleElement"/> class.
    /// </summary>
    /// <param name="baseValue">A <see cref="StormXElementValuePath"/>.</param>
    public StormStyleStyleElement(StormXElementValuePath baseValue)
        : base(baseValue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StormStyleStyleElement"/> class.
    /// </summary>
    /// <param name="baseValue">Another <see cref="StormElement"/> instance.</param>
    public StormStyleStyleElement(StormElement baseValue)
        : base(baseValue)
    {
    }

    /// <summary>
    /// Gets the value of the name attribute.
    /// </summary>
    public string? Name
    {
        get
        {
            if (HasName)
                return DataValues.ElementDataPairs[NameAttribute].Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets the value of the template attribute.
    /// </summary>
    public string? Template
    {
        get
        {
            if (HasTemplate)
                return DataValues.ElementDataPairs[TemplateAttribute].Value;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Name"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Name))]
    public bool HasName => DataValues.ElementDataPairs.ContainsKey(NameAttribute);

    /// <summary>
    /// Gets a value indicating whether <see cref="Template"/> exists or not.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Template))]
    public bool HasTemplate => DataValues.ElementDataPairs.ContainsKey(TemplateAttribute);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display;

            if (HasName && HasTemplate)
                display = $"<{ElementType} name=\"{Name}\" template=\"{Template}\">";
            else if (HasName && !HasTemplate)
                display = $"<{ElementType} name=\"{Name}\">";
            else if (!HasName && HasTemplate)
                display = $"<{ElementType} template=\"{Template}\">";
            else
                display = $"<{ElementType}>";

            return display;
        }
    }
}
