using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MioHelper.Currency
{
    /// <summary>
    /// Persistent currency store. Place one instance in the scene (usually a boot scene).
    /// Amounts are keyed by <see cref="CurrencyType"/> ScriptableObjects. Any change fires
    /// <see cref="OnCurrencyChanged"/> so <see cref="CurrencyDisplay"/> UIs can auto-update.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        [Header("Currency Manager")]
        [Tooltip("All currency types that will be used in this project.")]
        public List<CurrencyType> CurrencyTypes;
        public CurrencyTypeSpriteDictionary CurrencyIconTable;

        [Header("Debug")]
        public CurrencyType DebugCurrencyType;

        [Range(0f, 999f)]
        public int DebugCurrencyAmount;

        /// <summary>Fired whenever a currency amount is set: (currencyType, newAmount).</summary>
        public static event Action<CurrencyEvent> OnCurrencyChanged;

        [ContextMenu("Debug Change Currency")]
        void DebugChangeCurrency() => ChangeCurrency(DebugCurrencyType, DebugCurrencyAmount);

        private Dictionary<CurrencyType, int> _currencyTable = new Dictionary<CurrencyType, int>();

        protected override void Awake()
        {
            base.Awake();

            // Persist across scene loads (matches the former MMPersistentSingleton behavior).
            DontDestroyOnLoad(gameObject);

            // Initialize the dictionary
            _currencyTable = new Dictionary<CurrencyType, int>();
            foreach (CurrencyType currencyType in CurrencyTypes)
            {
                SetCurrency(currencyType, 0);
            }
        }


        public void ChangeCurrency(CurrencyField currencyField)
        {
            ChangeCurrency(currencyField.CurrencyType, currencyField.Amount);
        }

        public void ChangeCurrency(CurrencyType currencyType, int amount)
        {
            SetCurrency(currencyType, _currencyTable[currencyType] + amount);
        }


        public void SetCurrency(CurrencyField currencyField)
        {
            SetCurrency(currencyField.CurrencyType, currencyField.Amount);
        }

        public void SetCurrency(CurrencyType currencyType, int amount)
        {
            _currencyTable[currencyType] = amount;
            OnCurrencyChanged?.Invoke(new CurrencyEvent(currencyType, amount));
        }


        public bool EnoughCurrency(CurrencyField currencyField)
        {
            return EnoughCurrency(currencyField.CurrencyType, currencyField.Amount);
        }

        public bool EnoughCurrency(CurrencyType currencyType, int amount)
        {
            return _currencyTable.TryGetValue(currencyType, out int current) && current >= amount;
        }

        public int GetCurrencyAmount(CurrencyType currencyType)
        {
            return _currencyTable.TryGetValue(currencyType, out int amount) ? amount : 0;
        }


        public CurrencyType GetCurrencyType(string name) => CurrencyTypes.First(c => c.name == name);
    }

    [System.Serializable]
    public class CurrencyTypeSpriteDictionary : UnitySerializedDictionary<CurrencyType, Sprite> { }


    /// <summary>
    /// Payload describing one currency change. Kept as a plain struct (no framework event bus).
    /// </summary>
    public struct CurrencyEvent
    {
        public CurrencyType CurrencyType;
        public int Amount;
        public CurrencyField CurrencyField => new CurrencyField(CurrencyType, Amount);

        public CurrencyEvent(CurrencyField currencyField)
        {
            CurrencyType = currencyField.CurrencyType;
            Amount = currencyField.Amount;
        }

        public CurrencyEvent(CurrencyType currencyType, int amount)
        {
            CurrencyType = currencyType;
            Amount = amount;
        }
    }

    [System.Serializable]
    public class CurrencyField
    {
        public CurrencyType CurrencyType;
        public int Amount;
        public CurrencyField(CurrencyType currencyType, int amount)
        {
            CurrencyType = currencyType;
            Amount = amount;
        }
    }
}
