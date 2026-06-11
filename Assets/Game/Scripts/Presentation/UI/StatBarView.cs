using System.Collections;
using DengeGame.Application;
using DengeGame.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DengeGame.Presentation.UI
{
    /// <summary>
    /// Üstte tek bir ülke değerini gösterir: renk ikonu + kısa isim + sayı + dolum çubuğu.
    /// Kritik seviyede yalnızca renkle değil, sembol ve renkle uyarır (erişilebilirlik).
    /// </summary>
    public sealed class StatBarView : MonoBehaviour
    {
        private const int CriticalLow = 15;
        private const int CriticalHigh = 85;

        private CountryValue _value;
        private Image _fill;
        private TextMeshProUGUI _valueLabel;
        private TextMeshProUGUI _hintLabel;
        private RectTransform _fillRect;
        private int _current;

        public static StatBarView Create(Transform parent, CountryValue value, string shortName)
        {
            var go = new GameObject($"Stat_{value}", typeof(RectTransform), typeof(StatBarView));
            go.transform.SetParent(parent, false);
            var view = go.GetComponent<StatBarView>();
            view.Build(value, shortName);
            return view;
        }

        private void Build(CountryValue value, string shortName)
        {
            _value = value;
            var root = (RectTransform)transform;

            // İkon (renk kutusu) — üstte
            var icon = UiBuilder.CreateImage(root, "Icon", UiTheme.Accent);
            UiBuilder.Anchor(UiBuilder.Rect(icon), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-18, -36), new Vector2(18, 0));

            // Kısa isim
            var name = UiBuilder.CreateText(root, "Name", shortName, UiTheme.FontStat, UiTheme.TextMuted);
            UiBuilder.Anchor(UiBuilder.Rect(name), new Vector2(0, 1f), new Vector2(1, 1f),
                new Vector2(0, -64), new Vector2(0, -38));

            // Dolum çubuğu (arka + ön)
            var barBg = UiBuilder.CreateImage(root, "BarBg", UiTheme.Panel);
            UiBuilder.Anchor(UiBuilder.Rect(barBg), new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.30f),
                Vector2.zero, Vector2.zero);
            _fill = UiBuilder.CreateImage(barBg.transform, "Fill", UiTheme.StatFill);
            _fillRect = UiBuilder.Rect(_fill);
            _fillRect.anchorMin = new Vector2(0, 0);
            _fillRect.anchorMax = new Vector2(0.5f, 1);
            _fillRect.offsetMin = Vector2.zero; _fillRect.offsetMax = Vector2.zero;

            // Sayı
            _valueLabel = UiBuilder.CreateText(root, "Value", "50", UiTheme.FontStat, UiTheme.TextLight);
            UiBuilder.Anchor(UiBuilder.Rect(_valueLabel), new Vector2(0, 0f), new Vector2(1, 0.18f),
                Vector2.zero, Vector2.zero);

            // Kritik sembolü
            _hintLabel = UiBuilder.CreateText(root, "Hint", "", UiTheme.FontSmall, UiTheme.Warning);
            UiBuilder.Anchor(UiBuilder.Rect(_hintLabel), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.55f),
                new Vector2(-20, 0), new Vector2(20, 0));

            SetValue(50, animate: false);
        }

        public CountryValue Value => _value;

        public void SetValue(int value, bool animate)
        {
            _current = Mathf.Clamp(value, 0, 100);
            _valueLabel.text = _current.ToString();

            bool critical = _current <= CriticalLow || _current >= CriticalHigh;
            _fill.color = critical ? UiTheme.StatCritical : UiTheme.StatFill;
            _hintLabel.text = critical ? "!" : "";

            float target = _current / 100f;
            if (animate && !UiPreferences.ReduceMotion && isActiveAndEnabled)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateFill(target));
            }
            else
            {
                ApplyFill(target);
            }
        }

        private IEnumerator AnimateFill(float target)
        {
            float start = _fillRect.anchorMax.x;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                ApplyFill(Mathf.Lerp(start, target, t));
                yield return null;
            }
            ApplyFill(target);
        }

        private void ApplyFill(float ratio)
        {
            _fillRect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
        }
    }
}
