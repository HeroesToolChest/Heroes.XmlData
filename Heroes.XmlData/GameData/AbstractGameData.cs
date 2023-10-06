using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heroes.XmlData.GameData
{
    internal abstract class AbstractGameData
    {
        private readonly string _modsDirectoryPath;

        public AbstractGameData(string modsDirectoryPath)
        {
            _modsDirectoryPath = modsDirectoryPath;

            HotsBuild = int.MaxValue;
        }

        public AbstractGameData(string modsDirectoryPath, int hotsBuild)
        {
            _modsDirectoryPath = modsDirectoryPath;

            HotsBuild = hotsBuild;
        }

        /// <summary>
        /// Gets the build version number of the data or if unknown will assume a latest version.
        /// </summary>
        public int HotsBuild { get; }
    }
}
