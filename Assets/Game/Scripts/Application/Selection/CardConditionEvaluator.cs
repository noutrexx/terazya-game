using DengeGame.Domain;

namespace DengeGame.Application.Selection
{
    /// <summary>
    /// Bir kartın koşullarının mevcut oyun durumunu sağlayıp sağlamadığını değerlendirir (saf).
    /// PreviousEventId dolu kartlar normal havuzda görünmez; yalnızca zamanlanarak gelir.
    /// </summary>
    public static class CardConditionEvaluator
    {
        public static bool Matches(EventCard card, GameState state)
        {
            if (card == null || state == null) return false;

            if (state.CurrentTurn < card.MinTurn || state.CurrentTurn > card.MaxTurn) return false;
            if (!string.IsNullOrEmpty(card.PreviousEventId)) return false;

            if (card.RequiredFlags != null)
                foreach (var f in card.RequiredFlags)
                    if (!state.HasFlag(f)) return false;

            if (card.ForbiddenFlags != null)
                foreach (var f in card.ForbiddenFlags)
                    if (state.HasFlag(f)) return false;

            if (card.RequiredPolicies != null)
                foreach (var p in card.RequiredPolicies)
                    if (!state.ActivePolicyIds.Contains(p)) return false;

            if (card.ForbiddenPolicies != null)
                foreach (var p in card.ForbiddenPolicies)
                    if (state.ActivePolicyIds.Contains(p)) return false;

            if (card.ValueConditions != null)
                foreach (var vc in card.ValueConditions)
                    if (vc != null && !vc.IsSatisfied(state.Stats.Get(vc.Value))) return false;

            if (card.RelationshipConditions != null)
                foreach (var rc in card.RelationshipConditions)
                    if (rc != null && !rc.IsSatisfied(state.GetRelationship(rc.CharacterId))) return false;

            return true;
        }
    }
}
