using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application.Validation
{
    public enum IssueSeverity { Warning = 0, Error = 1 }

    /// <summary>Bir kart doğrulama bulgusu.</summary>
    public sealed class CardIssue
    {
        public IssueSeverity Severity { get; }
        public string CardId { get; }
        public string Message { get; }

        public CardIssue(IssueSeverity severity, string cardId, string message)
        {
            Severity = severity;
            CardId = cardId ?? "(id yok)";
            Message = message;
        }

        public override string ToString() => $"[{Severity}] {CardId}: {Message}";
    }

    /// <summary>
    /// Kart verisini saf olarak doğrular (Unity'siz, test edilebilir). Editor aracı ve testler
    /// aynı mantığı kullanır. Denge/içerik analizleri (OP kart vb.) Faz 11/12'ye aittir.
    /// </summary>
    public static class CardValidator
    {
        private const int MaxTitleLength = 60;
        private const int MaxDescriptionLength = 240;
        private const int MaxDecisionTextLength = 48;

        public static List<CardIssue> Validate(IReadOnlyList<EventCard> cards)
        {
            var issues = new List<CardIssue>();
            if (cards == null) return issues;

            // Tüm id'ler ve zamanlanan olaylar (zincir/ulaşılabilirlik için).
            var ids = new HashSet<string>();
            var duplicateIds = new HashSet<string>();
            foreach (var c in cards)
            {
                if (c == null || string.IsNullOrEmpty(c.Id)) continue;
                if (!ids.Add(c.Id)) duplicateIds.Add(c.Id);
            }
            foreach (var dup in duplicateIds)
                issues.Add(new CardIssue(IssueSeverity.Error, dup, "Tekrarlanan ID."));

            var scheduledTargets = CollectScheduledTargets(cards);

            foreach (var card in cards)
            {
                if (card == null)
                {
                    issues.Add(new CardIssue(IssueSeverity.Error, null, "Null kart."));
                    continue;
                }

                ValidateText(card, issues);
                ValidateRanges(card, issues);
                ValidateEffects(card, issues);
                ValidateDuplicateDecision(card, issues);
                ValidateChain(card, ids, scheduledTargets, issues);
            }

            return issues;
        }

        private static void ValidateText(EventCard card, List<CardIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(card.Id))
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "ID boş."));
            if (string.IsNullOrWhiteSpace(card.Title))
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "Başlık boş."));
            if (string.IsNullOrWhiteSpace(card.Description))
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "Açıklama boş."));
            if (string.IsNullOrWhiteSpace(card.LeftText))
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "Sol karar metni boş."));
            if (string.IsNullOrWhiteSpace(card.RightText))
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "Sağ karar metni boş."));

            if (card.Title != null && card.Title.Length > MaxTitleLength)
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, $"Başlık çok uzun (>{MaxTitleLength})."));
            if (card.Description != null && card.Description.Length > MaxDescriptionLength)
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, $"Açıklama çok uzun (>{MaxDescriptionLength})."));
            if (card.LeftText != null && card.LeftText.Length > MaxDecisionTextLength)
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, $"Sol karar metni çok uzun (>{MaxDecisionTextLength}), mobilde taşabilir."));
            if (card.RightText != null && card.RightText.Length > MaxDecisionTextLength)
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, $"Sağ karar metni çok uzun (>{MaxDecisionTextLength}), mobilde taşabilir."));
        }

        private static void ValidateRanges(EventCard card, List<CardIssue> issues)
        {
            if (card.MinTurn > card.MaxTurn)
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id, $"MinTurn ({card.MinTurn}) > MaxTurn ({card.MaxTurn})."));

            if (card.ValueConditions != null)
                foreach (var vc in card.ValueConditions)
                    if (vc != null && vc.Min > vc.Max)
                        issues.Add(new CardIssue(IssueSeverity.Error, card.Id, $"{vc.Value} koşulu min ({vc.Min}) > max ({vc.Max})."));

            if (card.RelationshipConditions != null)
                foreach (var rc in card.RelationshipConditions)
                    if (rc != null && rc.Min > rc.Max)
                        issues.Add(new CardIssue(IssueSeverity.Error, card.Id, $"İlişki koşulu ({rc.CharacterId}) min > max."));
        }

        private static void ValidateEffects(EventCard card, List<CardIssue> issues)
        {
            bool leftEmpty = card.LeftEffects == null || card.LeftEffects.Count == 0;
            bool rightEmpty = card.RightEffects == null || card.RightEffects.Count == 0;
            if (leftEmpty)
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, "Sol karar etkisiz."));
            if (rightEmpty)
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, "Sağ karar etkisiz."));

            CheckEffectList(card, card.LeftEffects, issues);
            CheckEffectList(card, card.RightEffects, issues);
        }

        private static void CheckEffectList(EventCard card, List<EffectData> list, List<CardIssue> issues)
        {
            if (list == null) return;
            foreach (var e in list)
            {
                if (e == null) { issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "Null etki.")); continue; }
                if (e.Kind == EffectKind.RandomValue && e.IntA > e.IntB)
                    issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "RandomValue etkisi min > max."));
                if ((e.Kind == EffectKind.ScheduleEvent) && e.IntA < 1)
                    issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "ScheduleEvent gecikmesi < 1."));
                if ((e.Kind == EffectKind.TimedValue) && e.IntB < 1)
                    issues.Add(new CardIssue(IssueSeverity.Error, card.Id, "TimedValue süresi < 1."));
            }
        }

        private static void ValidateDuplicateDecision(EventCard card, List<CardIssue> issues)
        {
            bool sameText = !string.IsNullOrEmpty(card.LeftText) && card.LeftText == card.RightText;
            if (sameText && EffectsEqual(card.LeftEffects, card.RightEffects))
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id, "İki karar tamamen aynı (metin + etki)."));
        }

        private static void ValidateChain(EventCard card, HashSet<string> ids,
            HashSet<string> scheduledTargets, List<CardIssue> issues)
        {
            // Zincir önceki olayı var olmalı.
            if (!string.IsNullOrEmpty(card.PreviousEventId) && !ids.Contains(card.PreviousEventId))
                issues.Add(new CardIssue(IssueSeverity.Error, card.Id,
                    $"Kırık zincir: önceki olay '{card.PreviousEventId}' bulunamadı."));

            // Zincir devamı kartı hiçbir yerden zamanlanmıyorsa ulaşılamaz.
            if (!string.IsNullOrEmpty(card.PreviousEventId) && !scheduledTargets.Contains(card.Id))
                issues.Add(new CardIssue(IssueSeverity.Warning, card.Id,
                    "Ulaşılamaz kart: PreviousEventId dolu ama hiçbir ScheduleEvent bu kartı zamanlamıyor."));

            // Zamanlanan hedef var olmalı.
            if (card.LeftEffects != null) CheckScheduleTargets(card, card.LeftEffects, ids, issues);
            if (card.RightEffects != null) CheckScheduleTargets(card, card.RightEffects, ids, issues);
        }

        private static void CheckScheduleTargets(EventCard card, List<EffectData> list,
            HashSet<string> ids, List<CardIssue> issues)
        {
            foreach (var e in list)
                if (e != null && e.Kind == EffectKind.ScheduleEvent &&
                    !string.IsNullOrEmpty(e.Text) && !ids.Contains(e.Text))
                    issues.Add(new CardIssue(IssueSeverity.Error, card.Id,
                        $"Kırık zincir: zamanlanan olay '{e.Text}' bulunamadı."));
        }

        private static HashSet<string> CollectScheduledTargets(IReadOnlyList<EventCard> cards)
        {
            var set = new HashSet<string>();
            foreach (var c in cards)
            {
                if (c == null) continue;
                AddTargets(c.LeftEffects, set);
                AddTargets(c.RightEffects, set);
            }
            return set;
        }

        private static void AddTargets(List<EffectData> list, HashSet<string> set)
        {
            if (list == null) return;
            foreach (var e in list)
                if (e != null && e.Kind == EffectKind.ScheduleEvent && !string.IsNullOrEmpty(e.Text))
                    set.Add(e.Text);
        }

        private static bool EffectsEqual(List<EffectData> a, List<EffectData> b)
        {
            int ca = a?.Count ?? 0, cb = b?.Count ?? 0;
            if (ca != cb) return false;
            for (int i = 0; i < ca; i++)
            {
                var x = a[i]; var y = b[i];
                if (x == null || y == null) return false;
                if (x.Kind != y.Kind || x.Value != y.Value || x.IntA != y.IntA ||
                    x.IntB != y.IntB || x.Bool != y.Bool || x.Text != y.Text) return false;
            }
            return true;
        }
    }
}
