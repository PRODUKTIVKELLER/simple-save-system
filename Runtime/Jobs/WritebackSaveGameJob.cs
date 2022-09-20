using Produktivkeller.SimpleSaveSystem.Configuration;
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Jobs
{
    public class WritebackSaveGameJob : ThreadedJob
    {
        private string   _savePath;
        private SaveGame _saveGame;

        public WritebackSaveGameJob(string savePath, SaveGame saveGame)
        {
            _savePath = savePath;
            _saveGame = saveGame;
        }

        protected override void ThreadFunction()
        {
            SaveGame saveGameToStore = _saveGame;
            string   savePathToStore = _savePath;

            File.WriteAllText(savePathToStore, JsonUtility.ToJson(saveGameToStore, true));

            saveGameToStore.WritebackAllScheduledWritebacks();
        }
    }
}
