namespace Heroes.StormData.Cache.Strings;

/// <summary>
/// Represents a text value from an <c>id=value</c> text file (a gamestring from a <c>gamestrings.txt</c> file or an asset text from an <c>assets.txt</c> file),
/// along with the file it was loaded from.
/// </summary>
/// <param name="Value">The text value (the part after the <c>=</c>). This can be <see cref="string.Empty"/> if the line has no value.</param>
/// <param name="StormFile">
/// The <see cref="Heroes.StormData.StormFile"/> that the value was loaded from. When the same <c>id</c> is loaded again, the new value and its file replace the earlier ones.
/// </param>
internal readonly record struct StringValue(string Value, StormFile StormFile);
