using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heroes.XmlData.XmlCore;

internal class XmlGameData
{
    public XmlMainData XmlMainData { get; } = new();

    public XmlMapData XmlMapData { get; } = new();
}
