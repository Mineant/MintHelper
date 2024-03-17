using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED

namespace Mineant.Currency
{
    [DefaultExecutionOrder(-100)]
    public class CurrencyManager : MMPersistentSingleton<CurrencyManager>
    {
        [Header("Currency Manager")]
        [Tooltip("All currency types that will be used in this project.")]
        public List<CurrencyType> CurrencyTypes;
        public CurrencyTypeSpriteDictionary CurrencyIconTable;

        [Header("Debug")]
        public CurrencyType DebugCurrencyType;

        [Range(0f, 999f)]
        public int DebugCurrencyAmount;

#if ODIN_INSPECTOR
        [Button]
#endif
        [ContextMenu("Debug Change Currency")]
        void DebugChangeCurrency() => ChangeCurrency(DebugCurrencyType, DebugCurrencyAmount);

        private Dictionary<CurrencyType, int> _currencyTable = new Dictionary<CurrencyType, int>();

        protected override void Awake()
        {
            base.Awake();

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
            CurrencyEvent.Trigger(currencyType, amount);
        }



        public bool EnoughCurrency(CurrencyField currencyField)
        {
            return EnoughCurrency(currencyField.CurrencyType, currencyField.Amount);
        }

        public bool EnoughCurrency(CurrencyType currencyType, int amount)
        {
            return _currencyTable[currencyType] >= amount;
        }



        public CurrencyType GetCurrencyType(string name) => CurrencyTypes.First(c => c.name == name);
    }

    [System.Serializable]
    public class CurrencyTypeSpriteDictionary : UnitySerializedDictionary<CurrencyType, Sprite> { }


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

        static CurrencyEvent e;

        public static void Trigger(CurrencyType currencyType, int amount)
        {
            e.CurrencyType = currencyType;
            e.Amount = amount;

            MMEventManager.TriggerEvent(e);
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

#endif