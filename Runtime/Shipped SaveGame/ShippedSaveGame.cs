using System.Linq;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem
{
    [CreateAssetMenu(fileName = "Shipped SaveGame", menuName = "PRODUKTIVKELLER/Simple Save System/Shipped SaveGame")]
    public class ShippedSaveGame : ScriptableObject
    {
        public int saveSlot;
        public string saveGameJson;

        public static bool ExistsShippedSaveGameForSlot(int saveSlot)
        {
            ShippedSaveGame[] shippedSaveGames = Resources.LoadAll<ShippedSaveGame>("");

            if (shippedSaveGames.Where(shippedSaveGame => shippedSaveGame.saveSlot == saveSlot).Count() > 1)
            {
                Debug.LogError("There are more than one shipped SaveGames for slot: " + saveSlot.ToString());
            }

            return shippedSaveGames.Where(shippedSaveGame => shippedSaveGame.saveSlot == saveSlot).Count() >= 1;
        }

        public static ShippedSaveGame GetShippedSaveGameForSlot(int saveSlot)
        {
            return Resources.LoadAll<ShippedSaveGame>("").Where(shippedSaveGame => shippedSaveGame.saveSlot == saveSlot).First();
        }
    }
}
