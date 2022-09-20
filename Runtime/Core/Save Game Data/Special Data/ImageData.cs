using System.IO;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData
{
    public class ImageData : SpecialData
    {
        private Texture2D _texture2D;

        public ImageData()
        {

        }

        public ImageData(Texture2D texture2D)
        {
            _texture2D = texture2D;
        }

        public Texture2D Texture2D
        {
            get
            {
                return _texture2D;
            }
        }

        public void FillFromImagePath(string filePath)
        {
            byte[] pngBytes = File.ReadAllBytes(filePath);

            Texture2D texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(pngBytes);

            _texture2D = texture2D;
        }

        public override void WriteData(string saveIdentification, SaveGame saveGame, string saveGameSpecificDataFolder)
        {
            byte[] pngBytes = _texture2D.EncodeToPNG();

            string fullFilePath = saveGameSpecificDataFolder + "/" + saveIdentification + ".png";

            saveGame.AddScheduledFileData(fullFilePath,
                pngBytes);

            saveGame.Set(saveIdentification, fullFilePath);
        }
    }
}
