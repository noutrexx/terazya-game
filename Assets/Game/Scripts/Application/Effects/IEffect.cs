using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application.Effects
{
    /// <summary>
    /// Bir karar (veya politika/kriz) tarafından oyun durumuna uygulanabilen tek bir etki.
    /// Faz 4'te kararlar, Faz 8'de politika/kriz, Faz 9'da son tetikleyiciler aynı omurgayı paylaşır.
    /// İki aşamalı: önce <see cref="Validate"/> (mutasyonsuz), tüm etkiler geçerliyse <see cref="Apply"/>.
    /// </summary>
    public interface IEffect
    {
        /// <summary>Veri geçerliliğini denetler. Durumu DEĞİŞTİRMEZ.</summary>
        Result Validate();

        /// <summary>Etkiyi oyun durumuna uygular. Yalnızca Validate başarılıysa çağrılır.</summary>
        void Apply(EffectContext context);
    }

    /// <summary>Etki uygulanırken paylaşılan bağlam: durum, rastgelelik ve değişiklik nedeni.</summary>
    public sealed class EffectContext
    {
        public GameState State { get; }
        public IRandomService Random { get; }
        public string Reason { get; }

        public EffectContext(GameState state, IRandomService random, string reason)
        {
            State = state;
            Random = random;
            Reason = reason;
        }
    }
}
