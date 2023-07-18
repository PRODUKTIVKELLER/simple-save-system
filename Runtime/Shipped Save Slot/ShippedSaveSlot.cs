using System.Linq;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Shipped_Save_Slot
{
    [CreateAssetMenu(fileName = "Shipped Save Slot - 0", menuName = "PRODUKTIVKELLER/Simple Save System/Shipped Save Slot")]
    public class ShippedSaveSlot : ScriptableObject
    {
        public int saveSlot;
        public string saveGameJson;

        public static bool ExistsForSlot(int saveSlot)
        {
            ShippedSaveSlot[] shippedSaveGames = Resources.LoadAll<ShippedSaveSlot>("");

            if (shippedSaveGames.Count(shippedSaveGame => shippedSaveGame.saveSlot == saveSlot) > 1)
            {
                Debug.LogError("There are more than one shipped save slot for slot: " + saveSlot);
            }

            return shippedSaveGames.Count(shippedSaveGame => shippedSaveGame.saveSlot == saveSlot) >= 1;
        }

        public static ShippedSaveSlot GetShippedSaveGameForSlot(int saveSlot)
        {
            return Resources.LoadAll<ShippedSaveSlot>("").First(shippedSaveGame => shippedSaveGame.saveSlot == saveSlot);
        }
    }
}
