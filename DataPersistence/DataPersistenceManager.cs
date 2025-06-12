using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Mineant.SaveSystem
{
    public class DataPersistenceManager<TManager, TFileDataHandler, TGameData> : MonoBehaviour
        where TManager : DataPersistenceManager<TManager, TFileDataHandler, TGameData>
        where TFileDataHandler : FileDataHandler<TGameData>, new()
        where TGameData : GameData, new()
    {
        [Header("Debugging")]
        [SerializeField] protected bool _disableSaveLoad = false;
        [SerializeField] protected bool _createNewGameIfLoadNull = false;
        [SerializeField] protected bool _useTestProfileId = false;
        [SerializeField] protected string _testSelectedProfileId = "test";

        [Header("File Storage Config")]
        [SerializeField] protected string _fileName;
        [SerializeField] protected bool _useEncryption;

        [Header("Auto Saving Configuration")]
        [SerializeField] protected bool _autoSaveGame;
        [SerializeField] protected float _autoSaveTimeSeconds = 60f;

        protected TGameData _gameData;
        protected List<IDataPersistence> _dataPersistenceObjects;
        protected TFileDataHandler _dataHandler;

        protected string _selectedProfileId = "";

        protected Coroutine _autoSaveCoroutine;

        public static TManager Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
                Destroy(this.gameObject);
                return;
            }
            Instance = (TManager)this;
            DontDestroyOnLoad(this.gameObject);

            if (_disableSaveLoad)
            {
                Debug.LogWarning("Data Persistence is currently disabled!");
            }

            this._dataHandler = new TFileDataHandler();
            this._dataHandler.Initialize(Application.persistentDataPath, _fileName, _useEncryption);

            InitializeSelectedProfileId();
        }

        protected virtual void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected virtual void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            this._dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadGame();

            // start up the auto saving coroutine
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }
            _autoSaveCoroutine = StartCoroutine(AutoSave());
        }

        public virtual void ChangeSelectedProfileId(string newProfileId)
        {
            // update the profile to use for saving and loading
            this._selectedProfileId = newProfileId;
            // load the game, which will use that profile, updating our game data accordingly
            LoadGame();
        }

        public virtual void DeleteProfileData(string profileId)
        {
            // delete the data for this profile id
            _dataHandler.Delete(profileId);
            // initialize the selected profile id
            InitializeSelectedProfileId();
            // reload the game so that our data matches the newly selected profile id
            LoadGame();
        }

        protected virtual void InitializeSelectedProfileId()
        {
            this._selectedProfileId = _dataHandler.GetMostRecentlyUpdatedProfileId();
            if (_useTestProfileId)
            {
                this._selectedProfileId = _testSelectedProfileId;
                Debug.LogWarning("Overrode selected profile id with test id: " + _testSelectedProfileId);
            }
        }

        public virtual void NewGame()
        {
            this._gameData = new TGameData();
        }

        public virtual void LoadGame()
        {
            // return right away if data persistence is disabled
            if (_disableSaveLoad)
            {
                return;
            }

            // load any saved data from a file using the data handler
            this._gameData = _dataHandler.Load(_selectedProfileId);

            // start a new game if the data is null and we're configured to initialize data for debugging purposes
            if (this._gameData == null && _createNewGameIfLoadNull)
            {
                NewGame();
            }

            // if no data can be loaded, don't continue
            if (this._gameData == null)
            {
                Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
                return;
            }

            // push the loaded data to all other scripts that need it
            foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(_gameData);
            }
        }

        public virtual void SaveGame()
        {
            // return right away if data persistence is disabled
            if (_disableSaveLoad)
            {
                return;
            }

            // if we don't have any data to save, log a warning here
            if (this._gameData == null)
            {
                Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
                return;
            }

            // pass the data to other scripts so they can update it
            foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(_gameData);
            }

            // timestamp the data so we know when it was last saved
            _gameData.lastUpdated = System.DateTime.Now.ToBinary();

            // save that data to a file using the data handler
            _dataHandler.Save(_gameData, _selectedProfileId);
        }

        protected virtual void OnApplicationQuit()
        {
            SaveGame();
        }

        protected virtual List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            // FindObjectsofType takes in an optional boolean to include inactive gameobjects
            IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
                .OfType<IDataPersistence>();

            return new List<IDataPersistence>(dataPersistenceObjects);
        }

        public virtual bool HasGameData()
        {
            return _gameData != null;
        }

        public virtual Dictionary<string, TGameData> GetAllProfilesGameData()
        {
            return _dataHandler.LoadAllProfiles();
        }

        protected virtual IEnumerator AutoSave()
        {
            if (!_autoSaveGame) yield break;
            
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveTimeSeconds);
                SaveGame();
                Debug.Log("Auto Saved Game");
            }
        }
    }
}