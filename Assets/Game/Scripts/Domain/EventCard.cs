using System;
using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>Bir kartın gösterilmesi için bir değerin bulunması gereken aralık.</summary>
    [Serializable]
    public sealed class ValueCondition
    {
        public CountryValue Value;
        public int Min = CountryValueInfo.Min;
        public int Max = CountryValueInfo.Max;

        public ValueCondition() { }
        public ValueCondition(CountryValue value, int min, int max) { Value = value; Min = min; Max = max; }

        public bool IsSatisfied(int current) => current >= Min && current <= Max;
    }

    /// <summary>Bir kartın gösterilmesi için bir ilişkinin bulunması gereken aralık.</summary>
    [Serializable]
    public sealed class RelationshipCondition
    {
        public string CharacterId;
        public int Min = GameState.RelationshipMin;
        public int Max = GameState.RelationshipMax;

        public RelationshipCondition() { }
        public RelationshipCondition(string characterId, int min, int max)
        {
            CharacterId = characterId; Min = min; Max = max;
        }

        public bool IsSatisfied(int current) => current >= Min && current <= Max;
    }

    /// <summary>
    /// Bir olay kartının saf C# domain modeli (tam şema). ScriptableObject authoring asset'i
    /// bu modele dönüştürülür; böylece domain Unity'siz test edilebilir kalır.
    /// Koşullar düz alanlardır; etkiler serileştirilebilir EffectData listeleridir.
    /// </summary>
    [Serializable]
    public sealed class EventCard
    {
        // Kimlik / metin
        public string Id;
        public string Title;
        public string CharacterId;
        public string Description;
        public string LeftText;
        public string RightText;

        // Etkiler (EffectFactory ile IEffect'e çevrilir)
        public List<EffectData> LeftEffects = new List<EffectData>();
        public List<EffectData> RightEffects = new List<EffectData>();

        // Sınıflandırma
        public string Category;
        public int Priority;
        public float Weight = 1f;

        // Zamanlama
        public int MinTurn = 0;
        public int MaxTurn = int.MaxValue;
        public bool OneShot;
        public int CooldownTurns;

        // Koşullar
        public List<string> RequiredFlags = new List<string>();
        public List<string> ForbiddenFlags = new List<string>();
        public List<string> RequiredPolicies = new List<string>();
        public List<string> ForbiddenPolicies = new List<string>();
        public List<ValueCondition> ValueConditions = new List<ValueCondition>();
        public List<RelationshipCondition> RelationshipConditions = new List<RelationshipCondition>();

        // Zincir
        public string ChainId;
        public string PreviousEventId; // doluysa kart yalnızca zamanlanarak gelir, normal havuzda görünmez

        // Bayraklar
        public bool IsEmergency;
        public bool IsFallback; // hiç uygun kart yoksa kullanılan güvenli kart

        public EventCard() { }

        public EventCard(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public bool HasEffects(DecisionSide side) =>
            (side == DecisionSide.Left ? LeftEffects : RightEffects)?.Count > 0;

        public IReadOnlyList<EffectData> EffectsFor(DecisionSide side) =>
            side == DecisionSide.Left ? LeftEffects : RightEffects;
    }
}
