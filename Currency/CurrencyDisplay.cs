using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MioHelper.Currency
{
    /// <summary>
    /// Displays one currency's icon + amount. With <see cref="AutoTrackCurrency"/> on, it
    /// subscribes to <see cref="CurrencyManager.OnCurrencyChanged"/> while enabled and updates
    /// whenever the tracked currency changes.
    /// </summary>
    public class CurrencyDisplay : MonoBehaviour
    {
        [Header("UI")]
        public Image IconImage;
        public TMP_Text ValueText;

        [Header("Auto")]
        public bool AutoTrackCurrency;
        public CurrencyType CurrencyToTrack;

        private CurrencyType _lastSetCurrency = null;

        void OnEnable()
        {
            if (AutoTrackCurrency)
            {
                CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
            }
        }

        void OnDisable()
        {
            if (AutoTrackCurrency)
            {
                CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void HandleCurrencyChanged(CurrencyEvent currencyEvent)
        {
            if (CurrencyToTrack == currencyEvent.CurrencyType)
            {
                Generate(currencyEvent.CurrencyType, currencyEvent.Amount);
            }
        }

        public void Generate(CurrencyField currencyField)
        {
            Generate(currencyField.CurrencyType, currencyField.Amount);
        }

        public void Generate(CurrencyType currencyType, int amount)
        {
            if (IconImage != null && (_lastSetCurrency == null || currencyType != _lastSetCurrency))
            {
                if (CurrencyManager.Instance != null && CurrencyManager.Instance.CurrencyIconTable != null
                    && CurrencyManager.Instance.CurrencyIconTable.TryGetValue(currencyType, out Sprite icon))
                {
                    IconImage.sprite = icon;
                }
            }

            if (ValueText != null) ValueText.text = amount + "";

            _lastSetCurrency = currencyType;
        }
    }
}
