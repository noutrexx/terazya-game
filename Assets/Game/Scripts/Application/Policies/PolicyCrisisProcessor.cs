using System.Collections.Generic;
using DengeGame.Application.Effects;
using DengeGame.Domain;

namespace DengeGame.Application.Policies
{
    /// <summary>
    /// Politika ve kriz yaşam döngüsünü her tur işler:
    /// - Politika: girişte StartEffects + uyumsuzları sonlandırma, her tur PerTurnEffects,
    ///   süre dolunca EndEffects + kaldırma.
    /// - Kriz: aşamalı kötüleşme (aşama süresince çözülmezse bir sonraki aşamaya geçer),
    ///   son aşama da çözülemezse FailEffects + belirli ihtimalle yönetim sonu.
    /// Saf C# (Unity'siz) → test edilebilir. GameLoop her AdvanceTurn'den sonra çağırır.
    /// </summary>
    public sealed class PolicyCrisisProcessor
    {
        private readonly PolicyRegistry _policies;
        private readonly CrisisRegistry _crises;

        public PolicyCrisisProcessor(PolicyRegistry policies, CrisisRegistry crises)
        {
            _policies = policies;
            _crises = crises;
        }

        public void Process(GameState state, IDecisionEffectService effects, IRandomService random)
        {
            if (state == null || state.IsEnded) return;
            ProcessPolicies(state, effects, random);
            if (!state.IsEnded) ProcessCrises(state, effects, random);
        }

        // --- Politikalar ---

        private void ProcessPolicies(GameState state, IDecisionEffectService effects, IRandomService random)
        {
            foreach (var ap in new List<ActivePolicy>(state.ActivePolicies))
            {
                if (state.IsEnded) return;
                var def = _policies?.Get(ap.PolicyId);
                if (def == null) { state.EndPolicy(ap.PolicyId); continue; }

                if (!ap.Entered)
                {
                    EndIncompatible(state, def, effects, random);
                    ap.RemainingTurns = def.DurationTurns;
                    ap.Entered = true;
                    Apply(state, def.StartEffects, effects, random, $"politika:{def.Id}:başlangıç");
                    continue;
                }

                Apply(state, def.PerTurnEffects, effects, random, $"politika:{def.Id}");

                if (def.DurationTurns > 0)
                {
                    ap.RemainingTurns--;
                    if (ap.RemainingTurns <= 0)
                    {
                        Apply(state, def.EndEffects, effects, random, $"politika:{def.Id}:bitiş");
                        state.EndPolicy(ap.PolicyId);
                    }
                }
            }
        }

        private void EndIncompatible(GameState state, PolicyDefinition def,
            IDecisionEffectService effects, IRandomService random)
        {
            if (def.IncompatiblePolicyIds == null) return;
            foreach (var otherId in def.IncompatiblePolicyIds)
            {
                if (!state.HasPolicy(otherId)) continue;
                var otherDef = _policies?.Get(otherId);
                if (otherDef != null)
                    Apply(state, otherDef.EndEffects, effects, random, $"politika:{otherId}:bitiş");
                state.EndPolicy(otherId);
            }
        }

        // --- Krizler ---

        private void ProcessCrises(GameState state, IDecisionEffectService effects, IRandomService random)
        {
            foreach (var ac in new List<ActiveCrisis>(state.ActiveCrises))
            {
                if (state.IsEnded) return;
                var def = _crises?.Get(ac.CrisisId);
                if (def == null || def.Stages == null || def.Stages.Count == 0)
                {
                    state.EndCrisis(ac.CrisisId);
                    continue;
                }

                if (!ac.Entered)
                {
                    ac.StageIndex = 0;
                    ac.TurnsInStage = 0;
                    ac.Entered = true;
                    Apply(state, def.Stages[0].EnterEffects, effects, random, $"kriz:{def.Id}:aşama1");
                    continue;
                }

                var stage = def.Stages[ac.StageIndex];
                Apply(state, stage.PerTurnEffects, effects, random, $"kriz:{def.Id}:aşama{ac.StageIndex + 1}");
                ac.TurnsInStage++;

                if (ac.TurnsInStage >= stage.DurationTurns)
                {
                    if (ac.StageIndex + 1 < def.Stages.Count)
                    {
                        ac.StageIndex++;
                        ac.TurnsInStage = 0;
                        Apply(state, def.Stages[ac.StageIndex].EnterEffects, effects, random,
                            $"kriz:{def.Id}:aşama{ac.StageIndex + 1}");
                    }
                    else
                    {
                        // Son aşama da çözülemedi → başarısızlık.
                        Apply(state, def.FailEffects, effects, random, $"kriz:{def.Id}:başarısız");
                        if (!string.IsNullOrEmpty(def.FailEndingReason) &&
                            random.NextDouble() < def.FailEndingChance)
                            state.EndGame(def.FailEndingReason);
                        state.EndCrisis(ac.CrisisId);
                    }
                }
            }
        }

        private static void Apply(GameState state, List<EffectData> data,
            IDecisionEffectService effects, IRandomService random, string reason)
        {
            if (data == null || data.Count == 0) return;
            var list = EffectFactory.CreateMany(data);
            effects.ApplyEffects(state, list, random, reason);
        }
    }
}
