using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application.Endings
{
    /// <summary>
    /// Bir karakterle ilişki uç eşiği aştığında özel yönetim sonu döndürür (örn. ordu darbesi).
    /// Karakterler id'ye göre sabit sırada denetlenir → deterministik.
    /// </summary>
    public sealed class CharacterEndingEvaluator : IEndingEvaluator
    {
        private readonly List<Character> _characters;

        public CharacterEndingEvaluator(IEnumerable<Character> characters)
        {
            _characters = new List<Character>(characters ?? new Character[0]);
            _characters.Sort((a, b) => string.CompareOrdinal(a?.Id, b?.Id));
        }

        public string Evaluate(GameState state)
        {
            if (state == null || state.IsEnded) return null;

            foreach (var c in _characters)
            {
                if (c == null || string.IsNullOrEmpty(c.Id)) continue;
                int rel = state.GetRelationship(c.Id);

                if (c.EnableLowEnding && rel <= c.LowEndingThreshold && !string.IsNullOrEmpty(c.LowEndingReason))
                    return c.LowEndingReason;
                if (c.EnableHighEnding && rel >= c.HighEndingThreshold && !string.IsNullOrEmpty(c.HighEndingReason))
                    return c.HighEndingReason;
            }
            return null;
        }
    }
}
