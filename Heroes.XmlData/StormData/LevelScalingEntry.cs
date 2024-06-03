using System.Diagnostics;

namespace Heroes.XmlData.StormData;

// for a <LevelScalingArray> element
// <Modifications>
//   <Catalog value="Behavior" />
//   <Entry value="AlexstraszaDragonqueenHealthIncrease" />
//   <Field value="Modification.VitalMaxArray[0]" />
//   <Value value="0.040000" />
//   <AffectedByOverdrive value="1" />
// </Modifications>
[DebuggerDisplay("{Catalog,nq},{Entry,nq},{Field,nq}")]
internal record LevelScalingEntry(string Catalog, string Entry, string Field);
