using System.Collections.Generic;
using DengeGame.Domain;

namespace DengeGame.Application.Effects
{
    /// <summary>
    /// Serileştirilebilir <see cref="EffectData"/>'yı çalıştırılabilir <see cref="IEffect"/>'e çevirir.
    /// Domain (veri) ile Application (davranış) arasındaki köprü; katman döngüsünü önler.
    /// </summary>
    public static class EffectFactory
    {
        public static IEffect Create(EffectData data)
        {
            if (data == null) return null;

            switch (data.Kind)
            {
                case EffectKind.ChangeValue:
                    return new ChangeValueEffect(data.Value, data.IntA);
                case EffectKind.RandomValue:
                    return new RandomValueEffect(data.Value, data.IntA, data.IntB);
                case EffectKind.SetFlag:
                    return new SetFlagEffect(data.Text, data.Bool);
                case EffectKind.StartPolicy:
                    return new StartPolicyEffect(data.Text);
                case EffectKind.StartCrisis:
                    return new StartCrisisEffect(data.Text);
                case EffectKind.EndCrisis:
                    return new EndCrisisEffect(data.Text);
                case EffectKind.ChangeRelationship:
                    return new ChangeRelationshipEffect(data.Text, data.IntA);
                case EffectKind.ScheduleEvent:
                    return new ScheduleEventEffect(data.Text, data.IntA);
                case EffectKind.TimedValue:
                    return new TimedValueEffect(data.Value, data.IntA, data.IntB);
                case EffectKind.EndGame:
                    return new EndGameEffect(data.Text);
                default:
                    return null;
            }
        }

        public static List<IEffect> CreateMany(IReadOnlyList<EffectData> data)
        {
            var list = new List<IEffect>();
            if (data == null) return list;
            foreach (var d in data)
            {
                var effect = Create(d);
                if (effect != null) list.Add(effect);
            }
            return list;
        }
    }
}
