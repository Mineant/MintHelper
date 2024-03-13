using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Mineant
{
    public class MintProgressBar : MonoBehaviour
    {
        public enum TextField { All, Fraction, Absolute, Percentage }

        [Header("Basic")]
        public Image ForegroundImage;

#if DOTWEEN
        [Header("Effects")]
        public bool ForegroundLerp;
        public Ease ForegroundEaseType = Ease.OutQuad;
        public float ForegroundLerpSpeed = 15f;
        public float ForegroundLerpDelay = 0f;
        protected Tween _foregroundTween;
#endif

        [Header("Text")]

        [Tooltip("Shows '10/100' if max is 100, current is 10, ignores min.")]
        public TMP_Text FractionValueText;
        public string FractionValueTextStringFormat;

        [Tooltip("Shows '54' if current value is 54")]
        public TMP_Text AbsoluteValueText;
        public string AbsoluteValueTextStringFormat;

        [Tooltip("Shows '20%' if current is 20, min is 0, max is 100")]
        public TMP_Text PercentageValueText;
        public string PercentageValueTextStringFormat;

        [Header("Debug")]
        [Range(0f, 1f)]
        public float DebugCurrent = 0.5f;
        public float DebugMin = 0;
        public float DebugMax = 100f;

        [ContextMenu("Debug Set Value")]
        void DebugSetValue() => SetValue(DebugCurrent * (DebugMax - DebugMin) + DebugMin, DebugMin, DebugMax);



        protected float _previousCurrentValue = Mathf.NegativeInfinity;

        public void SetValue(float current, float min, float max)
        {
            float adjustedCurrent = current - min;
            float adjustedMax = max - min;
            if (_previousCurrentValue == Mathf.NegativeInfinity) _previousCurrentValue = adjustedCurrent;

            bool usedDotween = false;

#if DOTWEEN
            if (ForegroundLerp)
            {
                usedDotween = true;

                // Kill the tween
                if (_foregroundTween != null) _foregroundTween.Kill();
                _foregroundTween = DOTween.To(
                    () => _previousCurrentValue,
                    (a) => ForegroundImage.fillAmount = a / adjustedMax,
                    adjustedCurrent, ForegroundLerpSpeed)
                    .SetEase(ForegroundEaseType)
                    .SetDelay(ForegroundLerpDelay)
                    .SetSpeedBased(true)
                    .Play();
            }

#endif

            if (!usedDotween) ForegroundImage.fillAmount = adjustedCurrent / adjustedMax;

            if (FractionValueText != null)
            {
                FractionValueText.text = $"{adjustedCurrent.ToString(FractionValueTextStringFormat)} / {adjustedMax.ToString(FractionValueTextStringFormat)}";
            }

            if (AbsoluteValueText != null)
            {
                AbsoluteValueText.text = $"{current.ToString(AbsoluteValueTextStringFormat)}";
            }

            if (PercentageValueText != null)
            {
                float percentage = adjustedCurrent / adjustedMax * 100f;
                PercentageValueText.text = $"{percentage.ToString(PercentageValueTextStringFormat)}%";
            }

            _previousCurrentValue = adjustedCurrent;
        }

        public void SetValue(float current, float max) => SetValue(current, 0f, max);

        public void SetTextFieldActive(bool active, TextField textField)
        {
            switch (textField)
            {
                case (TextField.All):
                    if (FractionValueText != null) FractionValueText.gameObject.SetActive(active);
                    if (PercentageValueText != null) PercentageValueText.gameObject.SetActive(active);
                    if (AbsoluteValueText != null) AbsoluteValueText.gameObject.SetActive(active);
                    break;
                case (TextField.Absolute):
                    if (AbsoluteValueText != null) AbsoluteValueText.gameObject.SetActive(active);
                    break;
                case (TextField.Percentage):
                    if (PercentageValueText != null) PercentageValueText.gameObject.SetActive(active);
                    break;
                case (TextField.Fraction):
                    if (FractionValueText != null) FractionValueText.gameObject.SetActive(active);
                    break;
            }
        }

    }

}
