using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Domain;
using DengeGame.Presentation.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Oyun sahnesinin kök kontrolcüsü: HUD'u kurar (üstte 6 değer, ortada kart, altta bilgi+butonlar)
    /// ve GameLoop'u sürer. Kart kaydırma/buton kararını döngüye iletir, değer/son olaylarını dinler.
    /// </summary>
    public sealed class GameSceneController : MonoBehaviour, ISceneController
    {
        private GameServices _services;
        private GameLoop _loop;

        private readonly Dictionary<CountryValue, StatBarView> _bars = new Dictionary<CountryValue, StatBarView>();
        private RectTransform _cardArea;
        private CardView _currentCard;
        private TextMeshProUGUI _info;

        public void Construct(GameServices services) => _services = services;

        private void Start()
        {
            if (_services == null) { Debug.LogError("[Denge] GameSceneController enjeksiyonu yok."); return; }
            _loop = _services.Flow.CurrentLoop;
            if (_loop == null)
            {
                Debug.LogError("[Denge] Aktif oyun döngüsü yok. Oyun sahnesi yeni oyundan sonra açılmalı.");
                return;
            }

            BuildHud();

            _loop.CardPresented += OnCardPresented;
            _loop.Ended += OnEnded;
            _loop.Session.State.Stats.Changed += OnStatChanged;

            RefreshAllStats();
            RefreshInfo();
            _loop.Begin();
        }

        private void OnDestroy()
        {
            if (_loop == null) return;
            _loop.CardPresented -= OnCardPresented;
            _loop.Ended -= OnEnded;
            _loop.Session.State.Stats.Changed -= OnStatChanged;
        }

        // --- HUD kurulumu ---

        private void BuildHud()
        {
            var safe = UiBuilder.CreateCanvas("GameUI", out var canvas);
            var bg = UiBuilder.CreateImage(canvas.transform, "Bg", UiTheme.Background);
            UiBuilder.Stretch(UiBuilder.Rect(bg));
            bg.transform.SetAsFirstSibling();

            // Üst: 6 değer
            var top = UiBuilder.CreateImage(safe, "TopBar", UiTheme.Panel);
            UiBuilder.Anchor(UiBuilder.Rect(top), new Vector2(0, 0.85f), new Vector2(1, 1f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < CountryValueInfo.Count; i++)
            {
                var v = CountryValueInfo.All[i];
                var bar = StatBarView.Create(top.transform, v, CountryValueLabels.Short(v));
                var r = (RectTransform)bar.transform;
                UiBuilder.Anchor(r, new Vector2(i / 6f, 0f), new Vector2((i + 1) / 6f, 1f),
                    new Vector2(6, 8), new Vector2(-6, -8));
                _bars[v] = bar;
            }

            // Orta: kart alanı
            var areaGo = new GameObject("CardArea", typeof(RectTransform));
            _cardArea = (RectTransform)areaGo.transform;
            _cardArea.SetParent(safe, false);
            UiBuilder.Anchor(_cardArea, new Vector2(0, 0.18f), new Vector2(1, 0.84f), Vector2.zero, Vector2.zero);

            // Alt: bilgi + butonlar
            var bottom = UiBuilder.CreateImage(safe, "BottomBar", UiTheme.Panel);
            UiBuilder.Anchor(UiBuilder.Rect(bottom), new Vector2(0, 0f), new Vector2(1, 0.17f), Vector2.zero, Vector2.zero);

            _info = UiBuilder.CreateText(bottom.transform, "Info", "", UiTheme.FontSmall, UiTheme.TextLight);
            UiBuilder.Anchor(UiBuilder.Rect(_info), new Vector2(0, 0.55f), new Vector2(1, 1f),
                new Vector2(24, 0), new Vector2(-24, 0));

            var settings = UiBuilder.CreateButton(bottom.transform, "Settings", "Ayarlar",
                UiTheme.FontSmall, UiTheme.Accent, UiTheme.TextLight, OnSettings);
            UiBuilder.Anchor(UiBuilder.Rect(settings), new Vector2(0.04f, 0.08f), new Vector2(0.30f, 0.5f), Vector2.zero, Vector2.zero);

            var history = UiBuilder.CreateButton(bottom.transform, "History", "Geçmiş",
                UiTheme.FontSmall, UiTheme.Accent, UiTheme.TextLight, OnHistory);
            UiBuilder.Anchor(UiBuilder.Rect(history), new Vector2(0.70f, 0.08f), new Vector2(0.96f, 0.5f), Vector2.zero, Vector2.zero);

            // Erişilebilirlik: kaydırma yerine sol/sağ karar butonları
            if (UiPreferences.ShowDecisionButtons)
            {
                var left = UiBuilder.CreateButton(bottom.transform, "LeftDecision", "◀",
                    UiTheme.FontBody, UiTheme.Negative, UiTheme.TextLight, () => Commit(DecisionSide.Left));
                UiBuilder.Anchor(UiBuilder.Rect(left), new Vector2(0.33f, 0.08f), new Vector2(0.49f, 0.5f), Vector2.zero, Vector2.zero);

                var right = UiBuilder.CreateButton(bottom.transform, "RightDecision", "▶",
                    UiTheme.FontBody, UiTheme.Positive, UiTheme.TextLight, () => Commit(DecisionSide.Right));
                UiBuilder.Anchor(UiBuilder.Rect(right), new Vector2(0.51f, 0.08f), new Vector2(0.67f, 0.5f), Vector2.zero, Vector2.zero);
            }
        }

        // --- Döngü olayları ---

        private void OnCardPresented(EventCard card)
        {
            if (_currentCard != null) Destroy(_currentCard.gameObject);

            _currentCard = CardView.Create(_cardArea);
            var r = (RectTransform)_currentCard.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(940, 1180);
            r.anchoredPosition = Vector2.zero;
            _currentCard.Setup(card);
            _currentCard.Committed += Commit;

            RefreshInfo();
        }

        private void Commit(DecisionSide side)
        {
            // Buton yolundan geliyorsa karta ilet (kaydırma animasyonu + tek taahhüt).
            if (_currentCard != null && _currentCard.Interactable)
            {
                _currentCard.ForceCommit(side);
                return;
            }
            // Karttan (Committed event) gelen çağrı: döngüye uygula.
            var old = _currentCard;
            _currentCard = null;
            var result = _loop.Decide(side);
            if (old != null && old != _currentCard) Destroy(old.gameObject);
            if (result.IsFailure)
                Debug.LogWarning($"[Denge] Karar uygulanamadı: {result.Error}");
        }

        private void OnEnded(string reason)
        {
            _services.Flow.EndCurrentGame(reason);
        }

        private void OnStatChanged(CountryStatChange change)
        {
            if (_bars.TryGetValue(change.Value, out var bar))
                bar.SetValue(change.Current, animate: true);
        }

        private void RefreshAllStats()
        {
            var stats = _loop.Session.State.Stats;
            foreach (var v in CountryValueInfo.All)
                if (_bars.TryGetValue(v, out var bar))
                    bar.SetValue(stats.Get(v), animate: false);
        }

        private void RefreshInfo()
        {
            var s = _loop.Session.State;
            _info.text = $"Tur {s.CurrentTurn}   ·   Yıl {s.CurrentYear}   ·   {s.CurrentPeriod}";
        }

        private void OnSettings() => Debug.Log("[Denge] Ayarlar (Faz 13'te gelecek).");
        private void OnHistory() => Debug.Log("[Denge] Geçmiş kararlar (Faz 9'da gelecek).");
    }
}
