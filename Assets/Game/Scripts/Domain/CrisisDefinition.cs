using System;
using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>Bir krizin tek aşaması: girişte ve her tur uygulanan etkiler + süresi.</summary>
    [Serializable]
    public sealed class CrisisStage
    {
        public string Name;
        public int DurationTurns = 2;
        public List<EffectData> EnterEffects = new List<EffectData>();
        public List<EffectData> PerTurnEffects = new List<EffectData>();
    }

    /// <summary>
    /// Bir krizin veri tanımı. Birden çok tur sürer, aşamalı olarak kötüleşir; krize özel kartlarla
    /// (EndCrisisEffect) çözülür. Son aşama da çözülemezse başarısızlık etkileri uygulanır ve
    /// belirli ihtimalle yönetim sonuna dönüşür.
    /// </summary>
    [Serializable]
    public sealed class CrisisDefinition
    {
        public string Id;
        public string Name;
        public string Description;

        public List<CrisisStage> Stages = new List<CrisisStage>();

        /// <summary>Son aşama çözülemezse uygulanacak etkiler.</summary>
        public List<EffectData> FailEffects = new List<EffectData>();

        /// <summary>Boş değilse, başarısızlıkta bu ihtimalle yönetim sonu tetiklenir.</summary>
        public string FailEndingReason;
        public float FailEndingChance; // 0..1

        public CrisisDefinition() { }
        public CrisisDefinition(string id, string name) { Id = id; Name = name; }
    }

    /// <summary>Aktif bir krizin runtime durumu.</summary>
    [Serializable]
    public sealed class ActiveCrisis
    {
        public string CrisisId;
        public int StageIndex;
        public int TurnsInStage;
        public bool Entered;

        public ActiveCrisis() { }
        public ActiveCrisis(string crisisId) { CrisisId = crisisId; }
    }
}
