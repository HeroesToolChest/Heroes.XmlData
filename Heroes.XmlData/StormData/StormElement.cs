using System.Diagnostics;

namespace Heroes.XmlData.StormData;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class StormElement
{
    private const string _idAttribute = "id";
    private const string _parentAttribute = "parent";

    public StormElement(StormXElementValuePath baseValue)
    {
        OriginalStormXElementValues.Add(baseValue);
        ElementType = baseValue.Value.Name.LocalName;

        DataValues = new StormElementData(baseValue.Value);
    }

    public StormElement(StormElement baseValue)
    {
        OriginalStormXElementValues = [.. baseValue.OriginalStormXElementValues];
        DataValues = new StormElementData(OriginalStormXElementValues[0].Value);

        for (int i = 1; i < OriginalStormXElementValues.Count; i++)
        {
            DataValues.AddXElement(OriginalStormXElementValues[i].Value);
        }

        ElementType = baseValue.ElementType;
    }

    public List<StormXElementValuePath> OriginalStormXElementValues { get; } = [];

    public StormElementData DataValues { get; private set; }

    public string ElementType { get; }

    public string? Id
    {
        get
        {
            if (HasId)
                return DataValues.KeyValueDataPairs[_idAttribute].Value;
            else
                return null;
        }
    }

    public string? ParentId
    {
        get
        {
            if (HasParentId)
                return DataValues.KeyValueDataPairs[_parentAttribute].Value;
            else
                return null;
        }
    }

    [MemberNotNullWhen(true, nameof(Id))]
    public bool HasId => DataValues.KeyValueDataPairs.ContainsKey(_idAttribute);

    [MemberNotNullWhen(true, nameof(ParentId))]
    public bool HasParentId => DataValues.KeyValueDataPairs.ContainsKey(_parentAttribute);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private string DebuggerDisplay
    {
        get
        {
            string display;

            if (HasId && HasParentId)
                display = $"<{ElementType} id=\"{Id}\" parent=\"{ParentId}\">";
            else if (HasId && !HasParentId)
                display = $"<{ElementType} id=\"{Id}\">";
            else if (!HasId && HasParentId)
                display = $"<{ElementType} parent=\"{ParentId}\">";
            else
                display = $"<{ElementType}>";

            return display;
        }
    }

    public void AddValue(StormXElementValuePath stormXElementValue)
    {
        OriginalStormXElementValues.Add(stormXElementValue);

        DataValues.AddXElement(stormXElementValue.Value);
    }

    public void AddValue(StormElement stormElement)
    {
        OriginalStormXElementValues.AddRange(stormElement.OriginalStormXElementValues);

        foreach (StormXElementValuePath item in stormElement.OriginalStormXElementValues)
        {
            DataValues.AddXElement(item.Value);
        }
    }
}
