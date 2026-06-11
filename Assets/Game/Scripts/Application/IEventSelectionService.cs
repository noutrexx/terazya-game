using System.Collections.Generic;
using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>
    /// Bir sonraki olay kartını seçen servis (port). Tam algoritma Faz 5'te uygulanır (GDD Bölüm 28).
    /// Aynı seed + aynı GameState ile deterministik olmak zorundadır.
    /// </summary>
    public interface IEventSelectionService
    {
        /// <summary>
        /// Verilen duruma uygun bir sonraki kartı seçer. Hiç uygun kart yoksa güvenli
        /// fallback kart döndürülür; gerçekten hiçbir kart yoksa Failure döner.
        /// </summary>
        Result<EventCard> SelectNext(GameState state, IReadOnlyList<EventCard> pool, IRandomService random);
    }
}
