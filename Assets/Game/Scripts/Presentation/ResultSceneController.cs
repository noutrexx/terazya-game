using System.Text;
using DengeGame.Application;
using DengeGame.Domain;
using DengeGame.Presentation.UI;
using TMPro;
using UnityEngine;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Sonuç ekranı: ulaşılan son, tur, son değerler, en destekçi/muhalif karakter, aktif politikalar,
    /// kriz durumu, yönetim tarzı, toplam skor (dökümüyle) + yeniden başlat/menü.
    /// </summary>
    public sealed class ResultSceneController : MonoBehaviour, ISceneController
    {
        private GameServices _services;

        public void Construct(GameServices services) => _services = services;

        private void Start()
        {
            if (_services == null) { Debug.LogError("[Denge] ResultSceneController enjeksiyonu yok."); return; }
            var session = _services.Flow.CurrentSession;
            if (session == null) { BuildMinimal(); return; }

            var state = session.State;
            var ending = _services.Endings.GetOrDefault(state.EndingReason);
            var score = ScoreCalculator.Calculate(state, ending.Rarity);
            state.SetScore(score.Total);

            var safe = UiBuilder.CreateCanvas("ResultUI", out var canvas);
            var bg = UiBuilder.CreateImage(canvas.transform, "Bg", UiTheme.Background);
            UiBuilder.Stretch(UiBuilder.Rect(bg)); bg.transform.SetAsFirstSibling();

            // Başlık (son tema rengi)
            var header = UiBuilder.CreateText(safe, "Header", ending.Title, UiTheme.FontTitle,
                CharacterAvatar.ToColor(ending.ThemeColor));
            UiBuilder.Anchor(UiBuilder.Rect(header), new Vector2(0, 0.90f), new Vector2(1, 1f),
                new Vector2(40, 0), new Vector2(-40, 0));

            // İçerik (kaydırılabilir)
            var content = UiBuilder.CreateScrollView(safe);
            UiBuilder.Anchor((RectTransform)content.parent, new Vector2(0, 0.14f), new Vector2(1, 0.89f),
                Vector2.zero, Vector2.zero);

            Label(content, $"<b>{CategoryText(ending.Category)} · {RarityText(ending.Rarity)}</b>");
            Label(content, ending.Description);
            Label(content, $"<b>Toplam Skor: {score.Total}</b>");
            Label(content, $"Tur {score.TurnScore} + Denge {score.BalanceScore} + Kriz {score.CrisisScore} + Zincir {score.ChainScore} + Son {score.EndingBonus}");
            Label(content, $"Görevde kalınan tur: {state.CurrentTurn}   ·   Yıl {state.CurrentYear}");
            Label(content, $"Yönetim tarzı: {ManagementStyle(state)}");
            Label(content, $"<b>Son Değerler</b>\n{FinalValues(state)}");
            Label(content, $"En çok destekleyen: {BestRelationship(state, true)}");
            Label(content, $"En çok karşı çıkan: {BestRelationship(state, false)}");
            Label(content, $"Aktif politikalar: {ActivePolicies(state)}");
            Label(content, $"Kriz — çözülen: {state.ResolvedCrisisCount}, başarısız: {state.FailedCrisisCount}, devam eden: {state.ActiveCrises.Count}");
            Label(content, $"Tamamlanan zincir: {state.CompletedChainCount}");
            Label(content, $"Verilen karar sayısı: {state.DecisionHistory.Count}");

            BuildButtons(safe);
        }

        private void BuildMinimal()
        {
            var safe = UiBuilder.CreateCanvas("ResultUI", out _);
            UiBuilder.CreateText(safe, "T", "Sonuç bulunamadı", UiTheme.FontTitle, UiTheme.TextLight);
            BuildButtons(safe);
        }

        private void BuildButtons(Transform parent)
        {
            var restart = UiBuilder.CreateButton(parent, "Restart", "Yeniden Başlat", UiTheme.FontBody,
                UiTheme.Accent, UiTheme.TextLight, () => _services.Flow.StartNewGame());
            UiBuilder.Anchor(UiBuilder.Rect(restart), new Vector2(0.06f, 0.02f), new Vector2(0.48f, 0.12f), Vector2.zero, Vector2.zero);

            var menu = UiBuilder.CreateButton(parent, "Menu", "Ana Menü", UiTheme.FontBody,
                UiTheme.Panel, UiTheme.TextLight, () => _services.Flow.ReturnToMenu());
            UiBuilder.Anchor(UiBuilder.Rect(menu), new Vector2(0.52f, 0.02f), new Vector2(0.94f, 0.12f), Vector2.zero, Vector2.zero);
        }

        private static TextMeshProUGUI Label(Transform content, string text)
        {
            var t = UiBuilder.CreateText(content, "Label", text, UiTheme.FontSmall, UiTheme.TextLight,
                TextAlignmentOptions.TopLeft);
            t.richText = true;
            var le = t.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            le.minHeight = 56;
            return t;
        }

        private static string FinalValues(GameState s)
        {
            var sb = new StringBuilder();
            foreach (var v in CountryValueInfo.All)
                sb.Append(CountryValueLabels.Full(v)).Append(": ").Append(s.Stats.Get(v)).Append("    ");
            return sb.ToString().TrimEnd();
        }

        private string BestRelationship(GameState s, bool highest)
        {
            string bestId = null; int best = highest ? int.MinValue : int.MaxValue;
            foreach (var pair in s.CharacterRelationships)
            {
                if (highest ? pair.Value > best : pair.Value < best) { best = pair.Value; bestId = pair.Key; }
            }
            if (bestId == null) return "—";
            string name = _services.Characters?.DisplayName(bestId) ?? bestId;
            return $"{name} ({RelationshipStatus.Of(best, _services.Characters?.Get(bestId)?.RelationshipTierNames)})";
        }

        private string ActivePolicies(GameState s)
        {
            if (s.ActivePolicies.Count == 0) return "yok";
            var sb = new StringBuilder();
            foreach (var p in s.ActivePolicies) sb.Append(p.PolicyId).Append(", ");
            return sb.ToString().TrimEnd(',', ' ');
        }

        private static string ManagementStyle(GameState s)
        {
            CountryValue top = CountryValue.Economy; int max = int.MinValue;
            foreach (var v in CountryValueInfo.All)
                if (s.Stats.Get(v) > max) { max = s.Stats.Get(v); top = v; }
            switch (top)
            {
                case CountryValue.Economy: return "Kalkınmacı";
                case CountryValue.PublicSupport: return "Popülist";
                case CountryValue.Security: return "Güvenlikçi";
                case CountryValue.Freedom: return "Özgürlükçü";
                case CountryValue.Environment: return "Çevreci";
                case CountryValue.Diplomacy: return "Diplomat";
                default: return "Dengeli";
            }
        }

        private static string CategoryText(EndingCategory c)
        {
            switch (c)
            {
                case EndingCategory.Disaster: return "Felaket";
                case EndingCategory.Success: return "Başarı";
                case EndingCategory.Ironic: return "İronik";
                default: return "Nötr";
            }
        }

        private static string RarityText(EndingRarity r) =>
            r == EndingRarity.Rare ? "Nadir" : (r == EndingRarity.Uncommon ? "Sıra dışı" : "Yaygın");
    }
}
