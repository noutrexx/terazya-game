using System;

namespace DengeGame.Domain
{
    /// <summary>Serileştirilebilir bir etkinin türü. EffectData bunlardan birini tanımlar.</summary>
    public enum EffectKind
    {
        ChangeValue = 0,
        RandomValue = 1,
        SetFlag = 2,
        StartPolicy = 3,
        StartCrisis = 4,
        EndCrisis = 5,
        ChangeRelationship = 6,
        ScheduleEvent = 7,
        TimedValue = 8,
        EndGame = 9,
        EndPolicy = 10
    }

    /// <summary>
    /// Bir etkinin SERİLEŞTİRİLEBİLİR veri temsili (ScriptableObject authoring için, saf C#).
    /// Application katmanındaki EffectFactory bunu çalıştırılabilir IEffect'e dönüştürür.
    /// Alanlar türe göre yorumlanır (aşağıdaki eşlemeye bakın).
    /// </summary>
    [Serializable]
    public sealed class EffectData
    {
        public EffectKind Kind;

        // ChangeValue/RandomValue/TimedValue için hedef değer.
        public CountryValue Value;

        // IntA: delta (ChangeValue) | min (RandomValue) | perTurnDelta (TimedValue)
        //       | turnsFromNow (ScheduleEvent) | delta (ChangeRelationship)
        public int IntA;

        // IntB: max (RandomValue) | durationTurns (TimedValue)
        public int IntB;

        // Text: flag (SetFlag) | policyId | crisisId | characterId | eventId | endingReason
        public string Text;

        // Bool: SetFlag etkinleştir/kapat
        public bool Bool;

        public EffectData() { }

        // --- Okunabilir kurucu yardımcıları (kod/test için) ---
        public static EffectData ChangeValue(CountryValue v, int delta) =>
            new EffectData { Kind = EffectKind.ChangeValue, Value = v, IntA = delta };

        public static EffectData RandomValue(CountryValue v, int min, int max) =>
            new EffectData { Kind = EffectKind.RandomValue, Value = v, IntA = min, IntB = max };

        public static EffectData SetFlag(string flag, bool enabled) =>
            new EffectData { Kind = EffectKind.SetFlag, Text = flag, Bool = enabled };

        public static EffectData StartPolicy(string id) =>
            new EffectData { Kind = EffectKind.StartPolicy, Text = id };

        public static EffectData StartCrisis(string id) =>
            new EffectData { Kind = EffectKind.StartCrisis, Text = id };

        public static EffectData EndCrisis(string id) =>
            new EffectData { Kind = EffectKind.EndCrisis, Text = id };

        public static EffectData ChangeRelationship(string characterId, int delta) =>
            new EffectData { Kind = EffectKind.ChangeRelationship, Text = characterId, IntA = delta };

        public static EffectData ScheduleEvent(string eventId, int turnsFromNow) =>
            new EffectData { Kind = EffectKind.ScheduleEvent, Text = eventId, IntA = turnsFromNow };

        public static EffectData TimedValue(CountryValue v, int perTurnDelta, int duration) =>
            new EffectData { Kind = EffectKind.TimedValue, Value = v, IntA = perTurnDelta, IntB = duration };

        public static EffectData EndGame(string reason) =>
            new EffectData { Kind = EffectKind.EndGame, Text = reason };

        public static EffectData EndPolicy(string policyId) =>
            new EffectData { Kind = EffectKind.EndPolicy, Text = policyId };
    }
}
