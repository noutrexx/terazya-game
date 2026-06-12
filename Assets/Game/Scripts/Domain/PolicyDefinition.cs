using System;
using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>
    /// Bir politikanın veri tanımı (ScriptableObject'ten çevrilir). Belirli kararlarla başlatılır,
    /// süre boyunca her tur etki uygular, bitince son etkilerini uygular.
    /// </summary>
    [Serializable]
    public sealed class PolicyDefinition
    {
        public string Id;
        public string Name;
        public string Description;

        public List<EffectData> StartEffects = new List<EffectData>();
        public List<EffectData> PerTurnEffects = new List<EffectData>();
        public List<EffectData> EndEffects = new List<EffectData>();

        /// <summary>Süre (tur). 0 = süresiz (yalnızca iptalle biter).</summary>
        public int DurationTurns;

        /// <summary>Bu politika başlayınca sonlandırılacak (uyumsuz) politikalar.</summary>
        public List<string> IncompatiblePolicyIds = new List<string>();

        public bool CanCancelEarly = true;

        public PolicyDefinition() { }
        public PolicyDefinition(string id, string name) { Id = id; Name = name; }
    }

    /// <summary>Aktif bir politikanın runtime durumu.</summary>
    [Serializable]
    public sealed class ActivePolicy
    {
        public string PolicyId;
        public int RemainingTurns = -1; // -1: henüz girilmedi (işleyici tanımdan ayarlar)
        public bool Entered;

        public ActivePolicy() { }
        public ActivePolicy(string policyId) { PolicyId = policyId; }
    }
}
