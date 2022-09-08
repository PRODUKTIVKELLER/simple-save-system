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
        public string   savePath;
        public SaveGame saveGame;

        protected override void ThreadFunction()
        {
            SaveGame saveGameToStore = saveGame;
            string   savePathToStore = savePath;

            using (var writer = new BinaryWriter(File.Open(savePathToStore, FileMode.Create)))
            {
                writer.Write(JsonUtility.ToJson(saveGameToStore, SaveSettings.Get().useJsonPrettyPrint));
            }
        }
    }
}
