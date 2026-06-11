using System.Collections.Generic;
using DengeGame.Application.Effects;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class EffectFactoryTests
    {
        [Test]
        public void Create_MapsEachKind_ToCorrectEffectType()
        {
            Assert.IsInstanceOf<ChangeValueEffect>(EffectFactory.Create(EffectData.ChangeValue(CountryValue.Economy, 5)));
            Assert.IsInstanceOf<RandomValueEffect>(EffectFactory.Create(EffectData.RandomValue(CountryValue.Economy, 1, 3)));
            Assert.IsInstanceOf<SetFlagEffect>(EffectFactory.Create(EffectData.SetFlag("f", true)));
            Assert.IsInstanceOf<StartPolicyEffect>(EffectFactory.Create(EffectData.StartPolicy("p")));
            Assert.IsInstanceOf<StartCrisisEffect>(EffectFactory.Create(EffectData.StartCrisis("c")));
            Assert.IsInstanceOf<EndCrisisEffect>(EffectFactory.Create(EffectData.EndCrisis("c")));
            Assert.IsInstanceOf<ChangeRelationshipEffect>(EffectFactory.Create(EffectData.ChangeRelationship("ch", 5)));
            Assert.IsInstanceOf<ScheduleEventEffect>(EffectFactory.Create(EffectData.ScheduleEvent("e", 2)));
            Assert.IsInstanceOf<TimedValueEffect>(EffectFactory.Create(EffectData.TimedValue(CountryValue.Economy, -2, 3)));
            Assert.IsInstanceOf<EndGameEffect>(EffectFactory.Create(EffectData.EndGame("son")));
        }

        [Test]
        public void CreateMany_SkipsNulls()
        {
            var data = new List<EffectData>
            {
                EffectData.ChangeValue(CountryValue.Economy, 1),
                null,
                EffectData.SetFlag("f", true)
            };
            var effects = EffectFactory.CreateMany(data);
            Assert.AreEqual(2, effects.Count);
        }

        [Test]
        public void CreatedEffects_ApplyThroughService_FromCardData()
        {
            // EffectData (kart verisi) → IEffect → DecisionEffectService
            var card = new EventCard("x", "X");
            card.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 10));
            card.LeftEffects.Add(EffectData.SetFlag("karar_verildi", true));

            var state = new GameState(1, new CountryStats());
            var effects = EffectFactory.CreateMany(card.EffectsFor(DecisionSide.Left));
            var result = new DecisionEffectService().ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(60, state.Stats.Get(CountryValue.Economy));
            Assert.IsTrue(state.HasFlag("karar_verildi"));
        }
    }
}
