using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mineant.Currency
{
    public class CurrencyDisplay : MonoBehaviour, MMEventListener<CurrencyEvent>
    {
        [Header("UI")]
        public Image IconImage;
        public TMP_Text ValueText;

        [Header("Auto")]
        public bool AutoTrackCurrencyCurrency;
        public CurrencyType CurrencyToTrack;

        private CurrencyType _lastSetCurrency = null; // Set to a immpossible number

        public void Generate(CurrencyField currencyField)
        {
            Generate(currencyField.CurrencyType, currencyField.Amount);
        }

        public void Generate(CurrencyType currencyType, int amount)
        {
            if (_lastSetCurrency == null || currencyType != _lastSetCurrency)
            {
                IconImage.sprite = CurrencyManager.Instance.CurrencyIconTable[currencyType];
            }

            ValueText.text = amount + "";

            _lastSetCurrency = currencyType;
        }

        public void OnMMEvent(CurrencyEvent eventType)
        {
            if (CurrencyToTrack == eventType.CurrencyType)
            {
                Generate(eventType.CurrencyType, eventType.Amount);
            }
        }

        void OnEnable()
        {
            if (AutoTrackCurrencyCurrency) this.MMEventStartListening<CurrencyEvent>();
        }

        void OnDisable()
        {
            if (AutoTrackCurrencyCurrency) this.MMEventStopListening<CurrencyEvent>();
        }
    }

}