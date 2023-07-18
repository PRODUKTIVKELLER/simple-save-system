using Produktivkeller.SimpleSaveSystem.Configuration;
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Produktivkeller.SimpleSaveSystem.Core.IO_Interface;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Jobs
{
    public class WritebackSaveGameJob : ThreadedJob
    {
        private string          _savePath;
        private SaveGame        _saveGame;
        private IFileReadWriter _fileReadWriter;

        public WritebackSaveGameJob(string savePath, SaveGame saveGame, IFileReadWriter fileReadWriter)
        {
            _savePath       = savePath;
            _saveGame       = saveGame;
            _fileReadWriter = fileReadWriter;
        }

        protected override void ThreadFunction()
        {
            SaveGame saveGameToStore = _saveGame;
            string   savePathToStore = _savePath;

            _fileReadWriter.WriteText(savePathToStore, JsonUtility.ToJson(saveGameToStore, true));

            saveGameToStore.WritebackAllScheduledWritebacks();
        }
    }
}
