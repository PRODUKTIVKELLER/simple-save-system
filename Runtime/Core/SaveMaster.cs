using Produktivkeller.SimpleSaveSystem.Configuration;
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData;
using Produktivkeller.SimpleSaveSystem.Migration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Produktivkeller.SimpleSaveSystem.Core
{
    /// <summary>
    /// Responsible for notifying all Saveable components
    /// Asking them to send data or retrieve data from/to the SaveGame
    /// </summary>
    [AddComponentMenu(""), DefaultExecutionOrder(-9015)]
    public class SaveMaster : MonoBehaviour
    {
        private static SaveMaster instance;

        private static GameObject saveMasterTemplate;

        // Used to track duplicate scenes.
        private static Dictionary<string, int> loadedSceneNames = new Dictionary<string, int>();
        private static HashSet<int> duplicatedSceneHandles = new HashSet<int>();

        private static bool isQuittingGame;

        // Active save game data
        private static SaveGame activeSaveGame = null;
        private static int activeSlot = -1;

        private Coroutine _coroutineTrackedPlaytime;

        // All listeners
        private static List<Saveable> saveables = new List<Saveable>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            GameObject saveMasterObject = new GameObject("Save Master");
            saveMasterObject.AddComponent<SaveMaster>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            GameObject.DontDestroyOnLoad(saveMasterObject);
        }

        /*  
        *  Instance managers exist to keep track of spawned objects.
        *  These managers make it possible to drop a coin, and when you reload the game
        *  the coin will still be there.
        */

        private static void OnSceneUnloaded(Scene scene)
        {
            if (activeSaveGame == null)
                return;

            // If it is a duplicate scene, we just remove this handle.
            if (duplicatedSceneHandles.Contains(scene.GetHashCode()))
            {
                duplicatedSceneHandles.Remove(scene.GetHashCode());
            }
            else
            {
                if (loadedSceneNames.ContainsKey(scene.name))
                {
                    loadedSceneNames.Remove(scene.name);
                }
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if (activeSaveGame == null)
                return;

            // Store a refeference to a non-duplicate scene
            if (!loadedSceneNames.ContainsKey(scene.name))
            {
                loadedSceneNames.Add(scene.name, scene.GetHashCode());
            }
            else
            {
                // These scenes are marked as duplicates. They need special treatment for saving.
                duplicatedSceneHandles.Add(scene.GetHashCode());
            }

            // Dont create save instance manager if there are no saved instances in the scene.
            if (string.IsNullOrEmpty(activeSaveGame.Get(string.Format("SaveMaster-{0}-IM", scene.name))))
            {
                return;
            }
        }

        /// <summary>
        /// Returns if the object has been destroyed using GameObject.Destroy(obj).
        /// Will return false if it has been destroyed due to the game exitting or scene unloading.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool DeactivatedObjectExplicitly(GameObject gameObject)
        {
            return gameObject.scene.isLoaded && !SaveMaster.isQuittingGame;
        }

        /// <summary>
        /// Returns the active slot. -1 means no slot is loaded
        /// </summary>
        /// <returns> Active slot </returns>
        public static int GetActiveSlot()
        {
            return activeSlot;
        }

        /// <summary>
        /// Checks if there are any unused save slots.
        /// </summary>
        /// <returns></returns>
        public static bool HasUnusedSlots()
        {
            return SaveFileUtility.GetAvailableSaveSlot() != -1;
        }

        public static int[] GetUsedSlots()
        {
            return SaveFileUtility.GetUsedSlots();
        }

        public static bool IsSlotUsed(int slot)
        {
            return SaveFileUtility.IsSlotUsed(slot);
        }

        /// <summary>
        /// Tries to set the current slot to the last used one.
        /// </summary>
        /// <param name="notifyListeners"> Should a load event be send to all active Saveables?</param>
        /// <returns> If it was able to set the slot to the last used one </returns>
        public static bool SetSlotToLastUsedSlot(bool notifyListeners)
        {
            int lastUsedSlot = PlayerPrefs.GetInt("SM-LastUsedSlot", -1);

            if (lastUsedSlot == -1)
            {
                return false;
            }
            else
            {
                SetSlot(lastUsedSlot, notifyListeners);
                return true;
            }
        }

        /// <summary>
        /// Attempts to set the slot to the first unused slot. Useful for creating a new game.
        /// </summary>
        /// <param name="notifyListeners"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static bool SetSlotToNewSlot(bool notifyListeners, out int slot)
        {
            int availableSlot = SaveFileUtility.GetAvailableSaveSlot();

            if (availableSlot == -1)
            {
                slot = -1;
                return false;
            }
            else
            {
                SetSlot(availableSlot, notifyListeners);
                slot = availableSlot;
                return true;
            }
        }

        /// <summary>
        /// Ensure save master has not been set to any slot
        /// </summary>
        public static void ClearSlot(bool clearAllListeners = true, bool notifySave = true)
        {
            if (clearAllListeners)
            {
                ClearListeners(notifySave);
            }

            activeSlot = -1;
            activeSaveGame = null;
        }

        /// <summary>
        /// Sets the slot, but does not save the data in the previous slot. This is useful if you want to
        /// save the active game to a new save slot. Like in older games such as Half-Life.
        /// </summary>
        /// <param name="slot"> Slot to switch towards, and copy the current save to </param>
        /// <param name="saveGame"> Set this if you want to overwrite a specific save file </param>
        public static void SetSlotAndCopyActiveSave(int slot)
        {
            OnSlotChangeBegin.Invoke(slot);

            activeSlot = slot;
            activeSaveGame = SaveFileUtility.LoadSave(slot, true);

            SyncReset();
            SyncSave();

            OnSlotChangeDone.Invoke(slot);
        }

        /// <summary>
        /// Set the active save slot. (Do note: If you don't want to auto save on slot switch, you can change this in the save setttings)
        /// </summary>
        /// <param name="slot"> Target save slot </param>
        /// <param name="reloadSaveables"> Send a message to all saveables to load the new save file </param>
        public static void SetSlot(int slot, bool reloadSaveables, SaveGame saveGame = null)
        {
            if (activeSlot == slot && saveGame == null)
            {
                Debug.LogWarning("Already loaded this slot.");
                return;
            }

            // Ensure the current game is saved, and write it to disk, if that is wanted behaviour.
            if (SaveSettings.Get().autoSaveOnSlotSwitch && activeSaveGame != null)
            {
                WriteActiveSaveToDisk();
            }

            if (slot < 0 || slot > SaveSettings.Get().maxSaveSlotCount)
            {
                Debug.LogWarning("SaveMaster: Attempted to set illegal slot.");
                return;
            }

            OnSlotChangeBegin.Invoke(slot);

            activeSlot = slot;
            activeSaveGame = (saveGame == null) ? SaveFileUtility.LoadSave(slot, true) : saveGame;

            if (reloadSaveables)
            {
                SyncLoad();
            }

            SyncReset();

            PlayerPrefs.SetInt("SM-LastUsedSlot", slot);

            OnSlotChangeDone.Invoke(slot);
        }

        public static DateTime GetSaveCreationTime(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.creationDate;
            }

            if (!IsSlotUsed(slot))
            {
                return new DateTime();
            }

            return GetSave(slot, true).creationDate;
        }

        public static DateTime GetSaveCreationTime()
        {
            return GetSaveCreationTime(activeSlot);
        }

        public static DateTime GetSaveLastSaveTime(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.lastSaveDate;
            }

            if (!IsSlotUsed(slot))
            {
                return new DateTime();
            }

            return GetSave(slot, true).lastSaveDate;
        }

        public static DateTime GetSaveLastSaveTime()
        {
            return GetSaveLastSaveTime(activeSlot);
        }

        public static void StampLastSaveTime()
        {
            if (activeSaveGame == null)
            {
                return;
            }

            activeSaveGame.lastSaveDate = DateTime.Now;
        }

        public static TimeSpan GetSaveTimePlayed(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.timePlayed;
            }

            if (!IsSlotUsed(slot))
            {
                return new TimeSpan();
            }

            return GetSave(slot, true).timePlayed;
        }

        public static TimeSpan GetSaveTimePlayed()
        {
            return GetSaveTimePlayed(activeSlot);
        }

        public static int GetSaveVersion(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.gameVersion;
            }

            if (!IsSlotUsed(slot))
            {
                return -1;
            }

            return GetSave(slot, true).gameVersion;
        }

        public static int GetSaveVersion()
        {
            return GetSaveVersion(activeSlot);
        }

        private static SaveGame GetSave(int slot, bool createIfEmpty = true)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame;
            }

            return SaveFileUtility.LoadSave(slot, createIfEmpty);
        }

        /// <summary>
        /// Automatically done on application quit or pause.
        /// Exposed in case you still want to manually write the active save.
        /// </summary>
        public static void WriteActiveSaveToDisk()
        {
            if (!AreSaveableIDsUnique())
            {
                Debug.LogError("Saving aborted due to duplicate saveable IDs");
                return;
            }

            OnWritingToDiskBegin.Invoke(activeSlot);

            if (activeSaveGame != null)
            {
                for (int i = 0; i < saveables.Count; i++)
                {
                    saveables[i].OnSaveRequest(activeSaveGame);
                }

                SaveFileUtility.WriteSave(activeSaveGame, activeSlot);
            }
            else
            {
                if (Time.frameCount != 0)
                {
                    Debug.Log("No save game is currently loaded... So we cannot save it");
                }
            }

            OnWritingToDiskDone.Invoke(activeSlot);
        }

        /// <summary>
        /// Wipe all data of the currently active savegame.
        /// </summary>
        public static void WipeAllData()
        {
            if (activeSaveGame == null)
            {
                Debug.LogError("Failed to wipe all data: No save game loaded.");
                return;
            }

            int listenerCount = saveables.Count;
            for (int i = listenerCount - 1; i >= 0; i--)
            {
                saveables[i].WipeData(activeSaveGame);
            }

            activeSaveGame.WipeAllData();
        }

        /// <summary>
        /// Wipe all data of a specified saveable
        /// </summary>
        /// <param name="saveable"></param>
        public static void WipeSaveable(Saveable saveable)
        {
            if (activeSaveGame == null)
            {
                Debug.LogError("Failed to wipe scene data: No save game loaded.");
                return;
            }

            saveable.WipeData(activeSaveGame);
        }

        /// <summary>
        /// Clears all saveable components that are listening to the Save Master
        /// </summary>
        /// <param name="notifySave"></param>
        public static void ClearListeners(bool notifySave)
        {
            if (notifySave && activeSaveGame != null)
            {
                int saveableCount = saveables.Count;
                for (int i = saveableCount - 1; i >= 0; i--)
                {
                    saveables[i].OnSaveRequest(activeSaveGame);
                }
            }

            saveables.Clear();
        }

        /// <summary>
        /// Useful in case components have been added to a saveable.
        /// </summary>
        /// <param name="saveable"></param>
        public static void ReloadListener(Saveable saveable)
        {
            saveable.OnLoadRequest(activeSaveGame);
        }

        /// <summary>
        /// Add saveable from the notification list. So it can recieve load/save requests.
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        public static void AddListener(Saveable saveable)
        {
            InitializeIfNeccessary();

            if (saveable != null && activeSaveGame != null)
            {
                saveable.OnLoadRequest(activeSaveGame);
            }

            saveables.Add(saveable);

            ValidateSaveableIDs();
        }

        /// <summary>
        /// Add saveable from the notification list. So it can recieve load/save requests.
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        public static void AddListener(Saveable saveable, bool loadData)
        {
            InitializeIfNeccessary();

            if (loadData)
            {
                AddListener(saveable);
            }
            else
            {
                saveables.Add(saveable);
            }

            ValidateSaveableIDs();
        }

        /// <summary>
        /// Remove saveable from the notification list. So it no longers recieves load/save requests.
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        public static void RemoveListener(Saveable saveable)
        {
            if (saveables.Remove(saveable))
            {
                if (saveable != null && activeSaveGame != null)
                {
                    saveable.OnSaveRequest(activeSaveGame);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        /// <param name="saveData"> Should it try to save the saveable data to the save file when being removed? </param>
        public static void RemoveListener(Saveable saveable, bool saveData)
        {
            if (saveData)
            {
                RemoveListener(saveable);
            }
            else
            {
                saveables.Remove(saveable);
            }
        }

        /// <summary>
        /// Delete a save file based on a specific slot.
        /// </summary>
        /// <param name="slot"></param>
        public static void DeleteSave(int slot)
        {
            SaveFileUtility.DeleteSave(slot);

            if (slot == activeSlot)
            {
                activeSlot = -1;
                activeSaveGame = null;
            }
        }

        /// <summary>
        /// Removes the active save file. Based on the save slot index.
        /// </summary>
        public static void DeleteSave()
        {
            DeleteSave(activeSlot);
        }

        /// <summary>
        /// Sends request to all saveables to store data to the active save game
        /// </summary>
        public static void SyncSave()
        {
            if (activeSaveGame == null)
            {
                Debug.LogWarning("SaveMaster Request Save Failed: " +
                                 "No active SaveGame has been set. Be sure to call SetSaveGame(index)");
                return;
            }

            int count = saveables.Count;

            for (int i = 0; i < count; i++)
            {
                saveables[i].OnSaveRequest(activeSaveGame);
            }
        }

        /// <summary>
        /// Sends request to all saveables to load data from the active save game
        /// </summary>
        public static void SyncLoad()
        {
            if (activeSaveGame == null)
            {
                Debug.LogWarning("SaveMaster Request Load Failed: " +
                                 "No active SaveGame has been set. Be sure to call SetSlot(index)");
                return;
            }

            int count = saveables.Count;

            for (int i = 0; i < count; i++)
            {
                saveables[i].OnLoadRequest(activeSaveGame);
            }
        }

        /// <summary>
        /// Resets the state of the saveables. As if they have never loaded or saved.
        /// </summary>
        public static void SyncReset()
        {
            if (activeSaveGame == null)
            {
                Debug.LogWarning("SaveMaster Request Load Failed: " +
                                 "No active SaveGame has been set. Be sure to call SetSlot(index)");
                return;
            }

            int count = saveables.Count;

            for (int i = 0; i < count; i++)
            {
                saveables[i].ResetState();
            }
        }

        /// <summary>
        /// Helper method for obtaining specific Saveable data.
        /// </summary>
        /// <typeparam name="T"> Object type to retrieve </typeparam>
        /// <param name="classType">Object type to retrieve</param>
        /// <param name="slot"> Save slot to load data from </param>
        /// <param name="saveableId"> Identification of saveable </param>
        /// <param name="componentId"> Identification of saveable component </param>
        /// <param name="data"> Data that gets returned </param>
        /// <returns></returns>
        public static bool GetSaveableData<T>(int slot, string saveableId, string componentId, out T data)
        {
            if (IsSlotUsed(slot) == false)
            {
                data = default(T);
                return false;
            }

            SaveGame saveGame = SaveMaster.GetSave(slot, false);

            if (saveGame == null)
            {
                data = default(T);
                return false;
            }

            string dataString = saveGame.Get(string.Format("{0}-{1}", saveableId, componentId));

            if (!string.IsNullOrEmpty(dataString))
            {
                data = JsonUtility.FromJson<T>(dataString);

                if (data != null)
                    return true;
            }

            data = default(T);
            return false;
        }

        /// <summary>
        /// Helper method for obtaining specific Saveable data.
        /// </summary>
        /// <typeparam name="T"> Object type to retrieve </typeparam>
        /// <param name="classType">Object type to retrieve</param>
        /// <param name="saveableId"> Identification of saveable </param>
        /// <param name="componentId"> Identification of saveable component </param>
        /// <param name="data"> Data that gets returned </param>
        /// <returns></returns>
        public static bool GetSaveableData<T>(string saveableId, string componentId, out T data)
        {
            if (activeSlot == -1)
            {
                data = default(T);
                return false;
            }

            return GetSaveableData<T>(activeSlot, saveableId, componentId, out data);
        }

        /// <summary>
        /// Set a integer value in the current currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="value"> Value to store </param>
        public static void SetInt(string key, int value)
        {
            if (HasActiveSaveSlot() == false) return;
            activeSaveGame.SetPrimitive(key, value);
        }

        /// <summary>
        /// Get a integer value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static int GetInt(string key, int defaultValue = -1)
        {
            if (HasActiveSaveSlot() == false) return defaultValue;
            return activeSaveGame.GetPrimitive<int>(key);
        }

        /// <summary>
        /// Set a floating point value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetFloat(string key, float value)
        {
            if (HasActiveSaveSlot() == false) return;
            activeSaveGame.SetPrimitive(key, value);
        }

        /// <summary>
        /// Get a float value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static float GetFloat(string key, float defaultValue = -1)
        {
            if (HasActiveSaveSlot() == false) return defaultValue;
            return activeSaveGame.GetPrimitive<float>(key);
        }

        /// <summary>
        /// Set a bool value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetBool(string key, bool value)
        {
            if (HasActiveSaveSlot() == false) return;
            activeSaveGame.SetPrimitive(key, value);
        }

        /// <summary>
        /// Get a bool value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (HasActiveSaveSlot() == false) return defaultValue;
            return activeSaveGame.GetPrimitive<bool>(key);
        }

        /// <summary>
        /// Set a string value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetString(string key, string value)
        {
            if (HasActiveSaveSlot() == false) return;
            activeSaveGame.SetPrimitive(key, value);
        }

        /// <summary>
        /// Get a string value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (HasActiveSaveSlot() == false) return defaultValue;
            return activeSaveGame.GetPrimitive<string>(key);
        }

        private static bool HasActiveSaveSlot()
        {
            if (SaveMaster.GetActiveSlot() == -1)
            {
                Debug.LogWarning("Failed: no save slot set. Please call SetSaveSlot(int index)");
                return false;
            }
            else return true;
        }

        private static bool AreSaveableIDsUnique()
        {
            bool areAllUnique = true;
            List<string> uniqueIdentifiers = new List<string>();
            for (int i = 0; i < saveables.Count; i++)
            {
                if (uniqueIdentifiers.Contains(saveables[i].SaveIdentification))
                {
                    areAllUnique = false;
                    Debug.LogError("Duplicate saveable identifier: [" + saveables[i].SaveIdentification + "]", saveables[i].gameObject);
                }
                uniqueIdentifiers.Add(saveables[i].SaveIdentification);

                for (int j = 0; j < saveables[i].AllComponentIDs.Count; j++)
                {
                    if (uniqueIdentifiers.Contains(saveables[i].AllComponentIDs[j]))
                    {
                        areAllUnique = false;
                        Debug.LogError("Duplicate saveable component identifier: [" + saveables[i].AllComponentIDs[j] + "]", saveables[i].gameObject);
                    }
                    uniqueIdentifiers.Add(saveables[i].AllComponentIDs[j]);
                }
            }

            return areAllUnique;
        }

        // Events

        /// <summary>
        /// Gets called after current saveables gets saved and written to disk.
        /// You can start loading scenes based on this callback.
        /// </summary>
        public static System.Action<int> OnSlotChangeBegin
        {
            get { return instance.onSlotChangeBegin; }
            set { instance.onSlotChangeBegin = value; }
        }

        public static System.Action<int> OnSlotChangeDone
        {
            get { return instance.onSlotChangeDone; }
            set { instance.onSlotChangeDone = value; }
        }

        public static System.Action<int> OnWritingToDiskBegin
        {
            get { return instance.onWritingToDiskBegin; }
            set { instance.onWritingToDiskBegin = value; }
        }

        public static System.Action<int> OnWritingToDiskDone
        {
            get { return instance.onWritingToDiskDone; }
            set { instance.onWritingToDiskDone = value; }
        }

        private System.Action<int> onSlotChangeBegin = delegate { };
        private System.Action<int> onSlotChangeDone = delegate { };
        private System.Action<int> onWritingToDiskBegin = delegate { };
        private System.Action<int> onWritingToDiskDone = delegate { };



        private static void InitializeIfNeccessary(SaveMaster saveMasterInstance)
        {
            if (instance != null
                && instance.GetInstanceID() != saveMasterInstance.GetInstanceID())
            {
                Debug.LogWarning("Duplicate save master found. " +
                                 "Ensure that the save master has not been added anywhere in your scene.");
                GameObject.Destroy(saveMasterInstance.gameObject);
                return;
            }

            if (instance != null && instance.GetInstanceID() == saveMasterInstance.GetInstanceID())
            {
                return;
            }

            instance = saveMasterInstance;

            if (instance._coroutineTrackedPlaytime != null)
            {
                instance.StopCoroutine(instance._coroutineTrackedPlaytime);
            }

            instance._coroutineTrackedPlaytime = instance.StartCoroutine(instance.TrackTimePlayed());

            var settings = SaveSettings.Get();

            if (settings.writebackToFileDisabled)
            {
                Debug.LogError("Writeback is disabled. Make sure to enable it again, otherwise no savegame will ever be stored to files");
            }

            if (settings.loadDefaultSlotOnStart)
            {
                SetSlot(settings.defaultSlot, true);
            }

            if (settings.useHotkeys)
            {
                instance.StartCoroutine(instance.TrackHotkeyUsage());
            }

            if (settings.saveOnInterval)
            {
                instance.StartCoroutine(instance.AutoSaveGame());
            }

            MigrationMaster.ProcessAllSavegames();
        }

        private static void InitializeIfNeccessary()
        {
            SaveMaster saveMaster = FindObjectOfType<SaveMaster>();
            InitializeIfNeccessary(saveMaster);
        }

        private void Awake()
        {
            InitializeIfNeccessary(this);
        }

        private void OnDestroy()
        {
            if (instance._coroutineTrackedPlaytime != null)
            {
                instance.StopCoroutine(instance._coroutineTrackedPlaytime);
            }
        }

        private static void ValidateSaveableIDs()
        {
            List<string> saveableIDs = new List<string>();
            List<string> componentIDs = new List<string>();
            for (int i = 0; i < saveables.Count; i++)
            {
                if (saveables[i].SaveIdentification.Length <= 0)
                {
                    Debug.LogError("Saveable ID is empty", saveables[i].gameObject);
                }

                if (saveableIDs.Contains(saveables[i].SaveIdentification))
                {
                    Debug.LogError("Duplicate saveable ID found: [" + saveables[i].SaveIdentification + "]", saveables[i].gameObject);
                }
                saveableIDs.Add(saveables[i].SaveIdentification);

                for (int j = 0; j < saveables[i].AllComponentIDs.Count; j++)
                {
                    if (componentIDs.Contains(saveables[i].AllComponentIDs[j]))
                    {
                        Debug.LogError("Duplicate saveable component ID found: [" + saveables[i].AllComponentIDs[j] + "]", saveables[i].gameObject);
                    }
                    componentIDs.Add(saveables[i].AllComponentIDs[j]);
                }
            }
        }

        private IEnumerator AutoSaveGame()
        {
            WaitForSeconds wait = new WaitForSeconds(SaveSettings.Get().saveIntervalTime);

            while (true)
            {
                yield return wait;
                WriteActiveSaveToDisk();
            }
        }

        private IEnumerator TrackHotkeyUsage()
        {
            var settings = SaveSettings.Get();

            while (true)
            {
                yield return null;

                if (!settings.useHotkeys)
                {
                    continue;
                }

                if (Input.GetKeyDown(settings.saveAndWriteToDiskKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    WriteActiveSaveToDisk();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced objects & Witten game to disk. MS: {0}", stopWatch.ElapsedMilliseconds.ToString()));
                }

                if (Input.GetKeyDown(settings.syncSaveGameKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    SyncSave();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced (Save) objects. MS: {0}", stopWatch.ElapsedMilliseconds.ToString()));
                }

                if (Input.GetKeyDown(settings.syncLoadGameKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    SyncLoad();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced (Load) objects. MS: {0}", stopWatch.ElapsedMilliseconds.ToString()));
                }
            }
        }

        private IEnumerator TrackTimePlayed()
        {
            var settings = SaveSettings.Get();

            while (true)
            {
                yield return new WaitForSecondsRealtime(1f);

                if (settings.trackTimePlayed
                    && activeSlot >= 0)
                {
                    activeSaveGame.timePlayed = activeSaveGame.timePlayed.Add(TimeSpan.FromSeconds(1f));

                    if (settings.showSaveFileUtilityLog)
                    {
                        Debug.Log("Played time: " + activeSaveGame.timePlayed.TotalSeconds.ToString());
                    }
                }
            }
        }

        // This will get called on android devices when they leave the game
        private void OnApplicationPause(bool pause)
        {
            if (!SaveSettings.Get().autoSaveOnExit)
                return;

            WriteActiveSaveToDisk();
        }

        private void OnApplicationQuit()
        {
            if (!SaveSettings.Get().autoSaveOnExit)
                return;

            isQuittingGame = true;
            WriteActiveSaveToDisk();
        }
    }
}