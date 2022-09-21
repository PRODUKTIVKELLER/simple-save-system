
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData;

namespace Produktivkeller.SimpleSaveSystem.Core
{
    public interface ISaveable
    {
        /// <summary>
        /// Called by a Saveable component. SaveMaster (request save) 
        /// -> notify to all Saveables -> return data to active save file with OnSave()
        /// </summary>
        /// <returns> Data for the save file </returns>
        string OnSave();

        SpecialData OnSaveSpecialData()
        {
            return null;
        }

        /// <summary>
        /// Called by a Saveable component. SaveMaster (request load) 
        /// -> notify to all Saveables -> obtain data for this specific component with OnLoad()
        /// </summary>
        /// <param name="data"> Data that gets retrieved from the active save file </param>
        void OnLoad(string data);

        /// <summary>
        /// Called by a Saveable component. SaveMaster (request load) 
        /// -> notify to all Saveables -> if no data for this ISaveable is present in the loaded savegame
        /// </summary>
        /// <param name="data"> Data that gets retrieved from the active save file </param>
        void OnNoDataToLoad();

        /// <summary>
        /// Returning true will allow the save to occur, else it will skip the save.
        /// This is useful when you want to call OnSave() only when something has actually changed.
        /// </summary>
        bool ShouldBeSaved();
    }
}