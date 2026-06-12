using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Application.Policies;
using DengeGame.Domain;
using NUnit.Framework;
using E = DengeGame.Domain.EffectData;
using V = DengeGame.Domain.CountryValue;

namespace DengeGame.Tests.EditMode
{
    public sealed class PolicyCrisisTests
    {
        private readonly DecisionEffectService _fx = new DecisionEffectService();

        private static List<E> L(params E[] e) => new List<E>(e);

        private static PolicyDefinition Ubi() => new PolicyDefinition("ubi", "UBI")
        {
            StartEffects = L(E.ChangeValue(V.PublicSupport, 8)),
            PerTurnEffects = L(E.ChangeValue(V.Economy, -2)),
            EndEffects = L(E.ChangeValue(V.PublicSupport, -4)),
            DurationTurns = 2
        };

        // --- Politika yaşam döngüsü ---

        [Test]
        public void Policy_AppliesStart_PerTurn_AndEnd_OverDuration()
        {
            var state = new GameState(1, new CountryStats());
            state.StartPolicy("ubi");
            var proc = new PolicyCrisisProcessor(new PolicyRegistry(new[] { Ubi() }), new CrisisRegistry(null));

            proc.Process(state, _fx, new StubRandom()); // giriş: PublicSupport +8
            Assert.AreEqual(58, state.Stats.Get(V.PublicSupport));
            Assert.AreEqual(50, state.Stats.Get(V.Economy));

            proc.Process(state, _fx, new StubRandom()); // her tur: Economy -2
            Assert.AreEqual(48, state.Stats.Get(V.Economy));

            proc.Process(state, _fx, new StubRandom()); // her tur -2 + süre doldu: end -4
            Assert.AreEqual(46, state.Stats.Get(V.Economy));
            Assert.AreEqual(54, state.Stats.Get(V.PublicSupport));
            Assert.IsFalse(state.HasPolicy("ubi"));
        }

        [Test]
        public void Policy_IncompatibleOne_IsEndedOnEnter()
        {
            var a = new PolicyDefinition("a", "A")
            {
                StartEffects = L(E.ChangeValue(V.Security, 5)),
                IncompatiblePolicyIds = new List<string> { "b" },
                DurationTurns = 5
            };
            var b = new PolicyDefinition("b", "B")
            {
                EndEffects = L(E.ChangeValue(V.Freedom, -3)),
                DurationTurns = 5
            };
            var state = new GameState(1, new CountryStats());
            state.StartPolicy("b");
            var proc = new PolicyCrisisProcessor(new PolicyRegistry(new[] { a, b }), new CrisisRegistry(null));
            proc.Process(state, _fx, new StubRandom()); // b girer
            state.StartPolicy("a");
            proc.Process(state, _fx, new StubRandom()); // a girer, b sonlanir (end -3)

            Assert.IsTrue(state.HasPolicy("a"));
            Assert.IsFalse(state.HasPolicy("b"));
            Assert.AreEqual(47, state.Stats.Get(V.Freedom));
        }

        [Test]
        public void EndPolicyEffect_CancelsPolicyEarly()
        {
            var state = new GameState(1);
            state.StartPolicy("ubi");
            var effects = EffectFactory.CreateMany(L(E.EndPolicy("ubi")));
            _fx.ApplyEffects(state, effects, new StubRandom());
            Assert.IsFalse(state.HasPolicy("ubi"));
        }

        // --- Kriz yaşam döngüsü ---

        private static CrisisDefinition Drought() => new CrisisDefinition("kuraklik", "Kuraklık")
        {
            Stages = new List<CrisisStage>
            {
                new CrisisStage { Name = "s1", DurationTurns = 1,
                    EnterEffects = L(E.ChangeValue(V.Economy, -3)), PerTurnEffects = L(E.ChangeValue(V.Economy, -1)) },
                new CrisisStage { Name = "s2", DurationTurns = 1,
                    EnterEffects = L(E.ChangeValue(V.Economy, -4)), PerTurnEffects = L(E.ChangeValue(V.Economy, -1)) }
            },
            FailEffects = L(E.ChangeValue(V.PublicSupport, -6)),
            FailEndingReason = "Susuzluk Felaketi",
            FailEndingChance = 1f
        };

        [Test]
        public void Crisis_Escalates_ThroughStages_ThenFails()
        {
            var state = new GameState(1, new CountryStats());
            state.StartCrisis("kuraklik");
            var proc = new PolicyCrisisProcessor(new PolicyRegistry(null), new CrisisRegistry(new[] { Drought() }));

            proc.Process(state, _fx, new StubRandom(doubleResult: 0.0)); // s1 girer: Eco -3 -> 47
            Assert.AreEqual(47, state.Stats.Get(V.Economy));
            Assert.AreEqual(0, state.GetActiveCrisis("kuraklik").StageIndex);

            proc.Process(state, _fx, new StubRandom(doubleResult: 0.0)); // s1 perturn -1 -> 46, sure dolar -> s2 girer -4 -> 42
            Assert.AreEqual(42, state.Stats.Get(V.Economy));
            Assert.AreEqual(1, state.GetActiveCrisis("kuraklik").StageIndex);

            proc.Process(state, _fx, new StubRandom(doubleResult: 0.0)); // s2 perturn -1 -> 41, son asama basarisiz
            Assert.IsTrue(state.IsEnded);
            Assert.AreEqual("Susuzluk Felaketi", state.EndingReason);
            Assert.IsFalse(state.HasCrisis("kuraklik"));
        }

        [Test]
        public void Crisis_Resolved_ByEndCrisis_DoesNotFail()
        {
            var state = new GameState(1, new CountryStats());
            state.StartCrisis("kuraklik");
            var proc = new PolicyCrisisProcessor(new PolicyRegistry(null), new CrisisRegistry(new[] { Drought() }));

            proc.Process(state, _fx, new StubRandom()); // s1 girer
            state.EndCrisis("kuraklik");               // çözüm kartı (EndCrisisEffect)
            proc.Process(state, _fx, new StubRandom()); // artık kriz yok

            Assert.IsFalse(state.IsEnded);
            Assert.IsFalse(state.HasCrisis("kuraklik"));
        }

        // --- Zincir (GameLoop entegrasyonu) ---

        [Test]
        public void Chain_ScheduledNextCard_IsPresented()
        {
            var a = new EventCard("a", "A");
            a.LeftEffects.Add(E.ScheduleEvent("b", 1));
            a.RightEffects.Add(E.ChangeValue(V.Economy, 1));
            var b = new EventCard("b", "B") { PreviousEventId = "a" };
            b.LeftEffects.Add(E.ChangeValue(V.Economy, 1));
            b.RightEffects.Add(E.ChangeValue(V.Economy, -1));

            var session = new GameSession(new GameState(1, new CountryStats()));
            var loop = new GameLoop(session, new List<EventCard> { a, b },
                new WeightedEventSelectionService(), _fx, new StubRandom(doubleResult: 0.0),
                new BoundaryEndingEvaluator());

            loop.Begin();
            Assert.AreEqual("a", loop.CurrentCard.Id); // b normalde havuzda yok (PreviousEventId)
            loop.Decide(DecisionSide.Left);            // b'yi zamanlar
            Assert.AreEqual("b", loop.CurrentCard.Id); // zamanlanan kart sunulur
        }
    }
}
