namespace Heroes.StormData.Xml;

/// <summary>
/// Represents a token declared by a <c>&lt;?token id="..." type="..." value="..."?&gt;</c> processing instruction.
/// </summary>
/// <param name="Id">The name of the token, as referenced by <c>##id##</c>.</param>
/// <param name="Type">The type of the token (e.g. <c>CHeroLink</c>), or an empty string if not specified.</param>
/// <param name="Value">The value of the token, or an empty string if not specified.</param>
public readonly record struct StormXmlToken(string Id, string Type, string Value);
