using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class TimedEffectTests
    {
        [Test]
        public void TimedEffect_AppliesEachTurn_ForItsDuration()
        {
            var state = new GameState(1, new CountryStats());
            var service = new DecisionEffectService();
            var manager = new TurnManager(state);

            // 3 tur boyunca her tur Ekonomi -5.
            service.ApplyEffects(state, new List<IEffect>
            {
                new TimedValueEffect(CountryValue.Economy, -5, 3)
            }, new StubRandom());

            Assert.AreEqual(50, state.Stats.Get(CountryValue.Economy)); // henüz uygulanmadı

            manager.AdvanceTurn(); // -5
            Assert.AreEqual(45, state.Stats.Get(CountryValue.Economy));
            manager.AdvanceTurn(); // -5
            manager.AdvanceTurn(); // -5
            Assert.AreEqual(35, state.Stats.Get(CountryValue.Economy));

            manager.AdvanceTurn(); // süre doldu, etki yok
            Assert.AreEqual(35, state.Stats.Get(CountryValue.Economy));
            Assert.AreEqual(0, state.ActiveTimedEffects.Count);
        }

        [Test]
        public void TimedEffect_ExpiresAndIsRemoved()
        {
            var state = new GameState(1, new CountryStats());
            var manager = new TurnManager(state);
            state.AddTimedEffect(new ActiveTimedEffect(CountryValue.Security, +2, 1, "test"));

            manager.AdvanceTurn();

            Assert.AreEqual(52, state.Stats.Get(CountryValue.Security));
            Assert.AreEqual(0, state.ActiveTimedEffects.Count);
        }
    }
}
