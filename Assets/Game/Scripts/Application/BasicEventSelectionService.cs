using System.Collections.Generic;
using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>
    /// Faz 3 için iskelet olay seçim servisi: temel tur/tek-kullanımlık filtresi + deterministik seçim.
    /// Faz 5'te GDD Bölüm 28'deki 9 adımlı tam algoritma (koşullar, ağırlık, kategori cezası,
    /// kriz/zincir önceliği) ile değiştirilecektir. Şimdiden deterministiktir.
    /// </summary>
    public sealed class BasicEventSelectionService : IEventSelectionService
    {
        public Result<EventCard> SelectNext(GameState state, IReadOnlyList<EventCard> pool, IRandomService random)
        {
            if (state == null) return Result.Failure<EventCard>("GameState null.");
            if (random == null) return Result.Failure<EventCard>("IRandomService null.");
            if (pool == null || pool.Count == 0)
                return Result.Failure<EventCard>("Kart havuzu boş.");

            var eligible = new List<EventCard>();
            foreach (var card in pool)
            {
                if (card == null || string.IsNullOrEmpty(card.Id)) continue;
                if (state.CurrentTurn < card.MinTurn || state.CurrentTurn > card.MaxTurn) continue;
                if (card.OneShot && state.ShownCardIds.Contains(card.Id)) continue;
                eligible.Add(card);
            }

            if (eligible.Count == 0)
                return Result.Failure<EventCard>("Uygun kart yok (fallback Faz 5'te eklenecek).");

            // Stabil sıralama (id) + seed'li seçim → deterministik.
            eligible.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            int index = random.NextInt(eligible.Count);
            return Result.Success(eligible[index]);
        }
    }
}
