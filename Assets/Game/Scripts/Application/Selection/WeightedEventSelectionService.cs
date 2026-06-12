using System.Collections.Generic;
using System.Linq;
using DengeGame.Application.Selection;
using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>
    /// GDD Bölüm 28'deki 9 adımlı olay seçim algoritması. Aynı seed + aynı GameState ile
    /// %100 deterministiktir (stabil ID sıralaması + tek IRandomService). Tekrar hissini
    /// cooldown, kategori cezası ve son-gösterilen filtresiyle azaltır.
    /// </summary>
    public sealed class WeightedEventSelectionService : IEventSelectionService
    {
        private const double CategoryPenalty = 0.35; // ardışık aynı kategoriyi seyrelt
        private const int RecentCategoryWindow = 2;
        private const double MinWeight = 0.01;

        public Result<EventCard> SelectNext(GameState state, IReadOnlyList<EventCard> pool, IRandomService random)
        {
            if (state == null) return Result.Failure<EventCard>("GameState null.");
            if (random == null) return Result.Failure<EventCard>("IRandomService null.");
            if (pool == null || pool.Count == 0) return Result.Failure<EventCard>("Kart havuzu boş.");

            var byId = new Dictionary<string, EventCard>();
            foreach (var c in pool)
                if (c != null && !string.IsNullOrEmpty(c.Id) && !byId.ContainsKey(c.Id))
                    byId[c.Id] = c;

            // 1) Zamanlanmış zorunlu olaylar (vadesi gelmiş).
            var scheduled = TryTakeScheduled(state, byId);
            if (scheduled != null) return Result.Success(scheduled);

            // 3,4,5) Koşul + yakın gösterim + tek-kullanımlık filtreleri.
            string lastShownId = state.DecisionHistory.Count > 0
                ? state.DecisionHistory[state.DecisionHistory.Count - 1].CardId
                : null;

            var eligible = pool.Where(c =>
                    c != null && !string.IsNullOrEmpty(c.Id) && !c.IsFallback &&
                    CardConditionEvaluator.Matches(c, state) &&
                    !(c.OneShot && state.ShownCardIds.Contains(c.Id)) &&
                    !IsOnCooldown(c, state) &&
                    c.Id != lastShownId)
                .ToList();

            // 2) Aktif kriz varsa acil kartları öne al; yoksa acil kartları dışarıda tut.
            List<EventCard> candidates;
            if (state.ActiveCrises.Count > 0)
            {
                var emergencies = eligible.Where(c => c.IsEmergency).ToList();
                candidates = emergencies.Count > 0 ? emergencies : eligible.Where(c => !c.IsEmergency).ToList();
            }
            else
            {
                candidates = eligible.Where(c => !c.IsEmergency).ToList();
            }

            // 9) Fallback.
            if (candidates.Count == 0)
                return Fallback(state, pool, random);

            // 6,7) Ağırlık + kategori cezası.
            var recentCategories = RecentCategories(state, byId);
            candidates.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id)); // stabil

            var weights = new double[candidates.Count];
            double total = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                double w = candidates[i].Weight <= 0 ? MinWeight : candidates[i].Weight;
                if (!string.IsNullOrEmpty(candidates[i].Category) && recentCategories.Contains(candidates[i].Category))
                    w *= CategoryPenalty;
                weights[i] = w;
                total += w;
            }

            // 8) Seed'li ağırlıklı seçim.
            if (total <= 0) return Result.Success(candidates[0]);
            double roll = random.NextDouble() * total;
            double acc = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                acc += weights[i];
                if (roll < acc) return Result.Success(candidates[i]);
            }
            return Result.Success(candidates[candidates.Count - 1]);
        }

        private static EventCard TryTakeScheduled(GameState state, Dictionary<string, EventCard> byId)
        {
            var due = state.ScheduledEvents
                .Select((se, idx) => (se, idx))
                .Where(x => x.se.DueTurn <= state.CurrentTurn)
                .OrderBy(x => x.se.DueTurn).ThenBy(x => x.idx)
                .ToList();

            foreach (var (se, _) in due)
            {
                if (byId.TryGetValue(se.EventId, out var card))
                {
                    state.ScheduledEvents.Remove(se);
                    return card;
                }
                // Havuzda yoksa kuyruktan düş (kırık zamanlama temizliği).
                state.ScheduledEvents.Remove(se);
            }
            return null;
        }

        private static bool IsOnCooldown(EventCard card, GameState state)
        {
            if (card.CooldownTurns <= 0) return false;
            if (!state.LastShownTurnByCard.TryGetValue(card.Id, out int lastTurn)) return false;
            return (state.CurrentTurn - lastTurn) < card.CooldownTurns;
        }

        private static HashSet<string> RecentCategories(GameState state, Dictionary<string, EventCard> byId)
        {
            var set = new HashSet<string>();
            int count = 0;
            for (int i = state.DecisionHistory.Count - 1; i >= 0 && count < RecentCategoryWindow; i--, count++)
            {
                var id = state.DecisionHistory[i].CardId;
                if (id != null && byId.TryGetValue(id, out var card) && !string.IsNullOrEmpty(card.Category))
                    set.Add(card.Category);
            }
            return set;
        }

        private static Result<EventCard> Fallback(GameState state, IReadOnlyList<EventCard> pool, IRandomService random)
        {
            var fallbacks = pool.Where(c => c != null && c.IsFallback && !string.IsNullOrEmpty(c.Id))
                                .OrderBy(c => c.Id, System.StringComparer.Ordinal).ToList();
            if (fallbacks.Count == 0)
                return Result.Failure<EventCard>("Uygun kart yok ve fallback kart tanımlı değil.");
            return Result.Success(fallbacks[random.NextInt(fallbacks.Count)]);
        }
    }
}
