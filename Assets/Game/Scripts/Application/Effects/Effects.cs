using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application.Effects
{
    /// <summary>Bir değere doğrudan delta uygular.</summary>
    public sealed class ChangeValueEffect : IEffect
    {
        public CountryValue Value { get; }
        public int Delta { get; }

        public ChangeValueEffect(CountryValue value, int delta)
        {
            Value = value;
            Delta = delta;
        }

        public Result Validate() => Result.Success();
        public void Apply(EffectContext context) => context.State.Stats.Apply(Value, Delta, context.Reason);
    }

    /// <summary>Bir değere [min, max] kapalı aralığında rastgele (seed'li) delta uygular.</summary>
    public sealed class RandomValueEffect : IEffect
    {
        public CountryValue Value { get; }
        public int Min { get; }
        public int Max { get; }

        public RandomValueEffect(CountryValue value, int min, int max)
        {
            Value = value;
            Min = min;
            Max = max;
        }

        public Result Validate() =>
            Min <= Max ? Result.Success() : Result.Failure($"RandomValueEffect: min ({Min}) > max ({Max}).");

        public void Apply(EffectContext context)
        {
            int delta = context.Random.NextInt(Min, Max + 1); // üst sınır dahil
            context.State.Stats.Apply(Value, delta, context.Reason);
        }
    }

    /// <summary>Bir bayrağı etkinleştirir veya kapatır.</summary>
    public sealed class SetFlagEffect : IEffect
    {
        public string Flag { get; }
        public bool Enabled { get; }

        public SetFlagEffect(string flag, bool enabled)
        {
            Flag = flag;
            Enabled = enabled;
        }

        public Result Validate() =>
            string.IsNullOrWhiteSpace(Flag) ? Result.Failure("SetFlagEffect: bayrak adı boş.") : Result.Success();

        public void Apply(EffectContext context) => context.State.SetFlag(Flag, Enabled);
    }

    /// <summary>Bir politikayı başlatır.</summary>
    public sealed class StartPolicyEffect : IEffect
    {
        public string PolicyId { get; }
        public StartPolicyEffect(string policyId) => PolicyId = policyId;

        public Result Validate() =>
            string.IsNullOrWhiteSpace(PolicyId) ? Result.Failure("StartPolicyEffect: politika id boş.") : Result.Success();

        public void Apply(EffectContext context) => context.State.StartPolicy(PolicyId);
    }

    /// <summary>Bir krizi başlatır.</summary>
    public sealed class StartCrisisEffect : IEffect
    {
        public string CrisisId { get; }
        public StartCrisisEffect(string crisisId) => CrisisId = crisisId;

        public Result Validate() =>
            string.IsNullOrWhiteSpace(CrisisId) ? Result.Failure("StartCrisisEffect: kriz id boş.") : Result.Success();

        public void Apply(EffectContext context) => context.State.StartCrisis(CrisisId);
    }

    /// <summary>Aktif bir krizi bitirir.</summary>
    public sealed class EndCrisisEffect : IEffect
    {
        public string CrisisId { get; }
        public EndCrisisEffect(string crisisId) => CrisisId = crisisId;

        public Result Validate() =>
            string.IsNullOrWhiteSpace(CrisisId) ? Result.Failure("EndCrisisEffect: kriz id boş.") : Result.Success();

        public void Apply(EffectContext context) => context.State.ResolveCrisis(CrisisId);
    }

    /// <summary>Bir karakterle ilişkiyi değiştirir (-100..100 sınırında).</summary>
    public sealed class ChangeRelationshipEffect : IEffect
    {
        public string CharacterId { get; }
        public int Delta { get; }

        public ChangeRelationshipEffect(string characterId, int delta)
        {
            CharacterId = characterId;
            Delta = delta;
        }

        public Result Validate() =>
            string.IsNullOrWhiteSpace(CharacterId)
                ? Result.Failure("ChangeRelationshipEffect: karakter id boş.")
                : Result.Success();

        public void Apply(EffectContext context) => context.State.ApplyRelationship(CharacterId, Delta);
    }

    /// <summary>Gelecekteki bir turda tetiklenecek olayı zamanlar.</summary>
    public sealed class ScheduleEventEffect : IEffect
    {
        public string EventId { get; }
        public int TurnsFromNow { get; }

        public ScheduleEventEffect(string eventId, int turnsFromNow)
        {
            EventId = eventId;
            TurnsFromNow = turnsFromNow;
        }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(EventId)) return Result.Failure("ScheduleEventEffect: olay id boş.");
            if (TurnsFromNow < 1) return Result.Failure("ScheduleEventEffect: gecikme en az 1 tur olmalı.");
            return Result.Success();
        }

        public void Apply(EffectContext context) =>
            context.State.ScheduleEvent(EventId, context.State.CurrentTurn + TurnsFromNow);
    }

    /// <summary>Belirli sayıda tur boyunca her tur bir değere delta uygulayan süreli etki kaydeder.</summary>
    public sealed class TimedValueEffect : IEffect
    {
        public CountryValue Value { get; }
        public int PerTurnDelta { get; }
        public int DurationTurns { get; }

        public TimedValueEffect(CountryValue value, int perTurnDelta, int durationTurns)
        {
            Value = value;
            PerTurnDelta = perTurnDelta;
            DurationTurns = durationTurns;
        }

        public Result Validate() =>
            DurationTurns < 1 ? Result.Failure("TimedValueEffect: süre en az 1 tur olmalı.") : Result.Success();

        public void Apply(EffectContext context) =>
            context.State.AddTimedEffect(new ActiveTimedEffect(Value, PerTurnDelta, DurationTurns, context.Reason));
    }

    /// <summary>Aktif bir politikayı erken iptal eder (durumdan kaldırır).</summary>
    public sealed class EndPolicyEffect : IEffect
    {
        public string PolicyId { get; }
        public EndPolicyEffect(string policyId) => PolicyId = policyId;

        public Result Validate() =>
            string.IsNullOrWhiteSpace(PolicyId) ? Result.Failure("EndPolicyEffect: politika id boş.") : Result.Success();

        public void Apply(EffectContext context) => context.State.EndPolicy(PolicyId);
    }

    /// <summary>Yönetimi doğrudan sonlandıran özel etki.</summary>
    public sealed class EndGameEffect : IEffect
    {
        public string EndingReason { get; }
        public EndGameEffect(string endingReason) => EndingReason = endingReason;

        public Result Validate() =>
            string.IsNullOrWhiteSpace(EndingReason) ? Result.Failure("EndGameEffect: sebep boş.") : Result.Success();

        public void Apply(EffectContext context) => context.State.EndGame(EndingReason);
    }
}
