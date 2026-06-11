using System.Collections.Generic;
using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application.Effects
{
    /// <summary>Bir kararın etkilerini oyun durumuna uygulayan tek servis (port).</summary>
    public interface IDecisionEffectService
    {
        Result ApplyEffects(GameState state, IReadOnlyList<IEffect> effects, IRandomService random, string reason = null);
    }

    /// <summary>
    /// Tüm karar etki uygulamasının tek noktası. Önce tüm etkiler doğrulanır; herhangi biri
    /// geçersizse durum HİÇ değiştirilmeden Failure döner (hatalı verinin güvenli reddi).
    /// Tüm etkiler geçerliyse hepsi sırayla uygulanır.
    /// </summary>
    public sealed class DecisionEffectService : IDecisionEffectService
    {
        public Result ApplyEffects(GameState state, IReadOnlyList<IEffect> effects, IRandomService random, string reason = null)
        {
            if (state == null) return Result.Failure("GameState null.");
            if (random == null) return Result.Failure("IRandomService null.");
            if (effects == null) return Result.Failure("Etki listesi null.");
            if (state.IsEnded) return Result.Failure("Yönetim sona erdi; karar uygulanamaz.");

            // Aşama 1 — doğrulama (mutasyon yok).
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                    return Result.Failure($"Etki[{i}] null; karar reddedildi.");

                var validation = effect.Validate();
                if (validation.IsFailure)
                    return Result.Failure($"Geçersiz etki[{i}]: {validation.Error}; karar reddedildi.");
            }

            // Aşama 2 — uygulama.
            var context = new EffectContext(state, random, reason);
            foreach (var effect in effects)
                effect.Apply(context);

            return Result.Success();
        }
    }
}
