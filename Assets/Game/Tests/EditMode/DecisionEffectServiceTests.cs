using System.Collections.Generic;
using DengeGame.Application.Effects;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class DecisionEffectServiceTests
    {
        private static GameState NewState() => new GameState(seed: 1, stats: new CountryStats());

        private readonly DecisionEffectService _service = new DecisionEffectService();

        [Test]
        public void Apply_MultipleEffects_InSingleDecision()
        {
            var state = NewState();
            var effects = new List<IEffect>
            {
                new ChangeValueEffect(CountryValue.Economy, +10),
                new ChangeValueEffect(CountryValue.Security, -5),
                new SetFlagEffect("reform_basladi", true)
            };

            var result = _service.ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(60, state.Stats.Get(CountryValue.Economy));
            Assert.AreEqual(45, state.Stats.Get(CountryValue.Security));
            Assert.IsTrue(state.HasFlag("reform_basladi"));
        }

        [Test]
        public void Apply_ChangeValue_IsClampedToBounds()
        {
            var state = new GameState(1, new CountryStats(95));
            var effects = new List<IEffect> { new ChangeValueEffect(CountryValue.Economy, +20) };

            _service.ApplyEffects(state, effects, new StubRandom());

            Assert.AreEqual(100, state.Stats.Get(CountryValue.Economy));
        }

        [Test]
        public void Apply_RandomValue_UsesSeededRandomDeterministically()
        {
            var state = NewState();
            var effects = new List<IEffect> { new RandomValueEffect(CountryValue.Diplomacy, 3, 8) };

            _service.ApplyEffects(state, effects, new StubRandom(intResult: 5));

            Assert.AreEqual(55, state.Stats.Get(CountryValue.Diplomacy)); // 50 + 5
        }

        [Test]
        public void Apply_ChangeRelationship_IsClamped()
        {
            var state = NewState();
            var effects = new List<IEffect> { new ChangeRelationshipEffect("general", -150) };

            _service.ApplyEffects(state, effects, new StubRandom());

            Assert.AreEqual(GameState.RelationshipMin, state.GetRelationship("general"));
        }

        [Test]
        public void Apply_StartPolicyAndCrisis()
        {
            var state = NewState();
            var effects = new List<IEffect>
            {
                new StartPolicyEffect("ubi"),
                new StartCrisisEffect("kuraklik")
            };

            _service.ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(state.HasPolicy("ubi"));
            Assert.IsTrue(state.HasCrisis("kuraklik"));
        }

        [Test]
        public void Apply_ScheduleEvent_QueuesForFutureTurn()
        {
            var state = NewState();
            state.AdvanceTurn(); // tur 1
            var effects = new List<IEffect> { new ScheduleEventEffect("zincir_2", 3) };

            _service.ApplyEffects(state, effects, new StubRandom());

            Assert.AreEqual(1, state.ScheduledEvents.Count);
            Assert.AreEqual("zincir_2", state.ScheduledEvents[0].EventId);
            Assert.AreEqual(4, state.ScheduledEvents[0].DueTurn); // 1 + 3
        }

        [Test]
        public void Apply_EndGameEffect_EndsManagement()
        {
            var state = NewState();
            var effects = new List<IEffect> { new EndGameEffect("Devlet İflası") };

            var result = _service.ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(state.IsEnded);
            Assert.AreEqual("Devlet İflası", state.EndingReason);
        }

        [Test]
        public void Apply_AfterGameEnded_IsRejected()
        {
            var state = NewState();
            state.EndGame("önceki son");
            var effects = new List<IEffect> { new ChangeValueEffect(CountryValue.Economy, +10) };

            var result = _service.ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(50, state.Stats.Get(CountryValue.Economy)); // değişmedi
        }

        // --- Hatalı verilerin güvenli reddi (mutasyon yok) ---

        [Test]
        public void Apply_InvalidEffect_RejectsWithoutMutatingState()
        {
            var state = NewState();
            var effects = new List<IEffect>
            {
                new ChangeValueEffect(CountryValue.Economy, +10), // geçerli
                new RandomValueEffect(CountryValue.Security, 9, 2) // GEÇERSİZ (min > max)
            };

            var result = _service.ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(result.IsFailure);
            // İlk etki geçerli olsa da, hiçbiri uygulanmamalı (iki aşamalı doğrulama).
            Assert.AreEqual(50, state.Stats.Get(CountryValue.Economy));
        }

        [Test]
        public void Apply_NullEffectInList_IsRejected()
        {
            var state = NewState();
            var effects = new List<IEffect> { new ChangeValueEffect(CountryValue.Economy, +10), null };

            var result = _service.ApplyEffects(state, effects, new StubRandom());

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(50, state.Stats.Get(CountryValue.Economy));
        }

        [Test]
        public void Apply_NullArguments_AreRejected()
        {
            var state = NewState();
            Assert.IsTrue(_service.ApplyEffects(null, new List<IEffect>(), new StubRandom()).IsFailure);
            Assert.IsTrue(_service.ApplyEffects(state, null, new StubRandom()).IsFailure);
            Assert.IsTrue(_service.ApplyEffects(state, new List<IEffect>(), null).IsFailure);
        }
    }
}
