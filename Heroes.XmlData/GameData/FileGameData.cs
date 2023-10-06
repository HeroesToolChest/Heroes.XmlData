using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heroes.XmlData.GameData
{
    internal class FileGameData : AbstractGameData
    {
        public FileGameData(string modsDirectoryPath)
            : base(modsDirectoryPath)
        {
        }

        public FileGameData(string modsDirectoryPath, int hotsBuild)
            : base(modsDirectoryPath, hotsBuild)
        {
        }
    }
}
