using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData
{
    public abstract class SpecialData
    {
        /// <summary>
        /// Instead of SaveGame.Set(id, value), this method is called and is completely responsible to save the given data.
        /// </summary>
        /// <param name="saveIdentification"></param>
        /// <param name="saveGame"></param>
        /// <param name="saveGameSpecificDataFolder"></param>
        /// <returns></returns>
        public abstract void WriteData(string saveIdentification, SaveGame saveGame, string saveGameSpecificDataFolder);
    }
}
