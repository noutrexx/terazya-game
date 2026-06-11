using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class WeightedEventSelectionTests
    {
        private readonly WeightedEventSelectionService _svc = new WeightedEventSelectionService();

        private static EventCard Card(string id) => new EventCard(id, id);

        [Test]
        public void Scheduled_TakesPriority_AndIsConsumed()
        {
            var chain = Card("zincir_devam");
            chain.PreviousEventId = "zincir_basi"; // normalde havuzda görünmez
            var pool = new List<EventCard> { Card("normal"), chain };

            var state = new GameState(1);
            state.AdvanceTurn(); // tur 1
            state.ScheduleEvent("zincir_devam", state.CurrentTurn);

            var result = _svc.SelectNext(state, pool, new StubRandom());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("zincir_devam", result.Value.Id);
            Assert.AreEqual(0, state.ScheduledEvents.Count); // tüketildi
        }

        [Test]
        public void ValueCondition_FiltersOutNonMatchingCard()
        {
            var low = Card("guvenlik_dusuk");
            low.ValueConditions.Add(new ValueCondition(CountryValue.Security, 0, 40));
            var pool = new List<EventCard> { Card("normal"), low };

            var state = new GameState(1); // Security 50 → 'low' elenmeli
            var result = _svc.SelectNext(state, pool, new StubRandom(doubleResult: 0.0));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("normal", result.Value.Id);
        }

        [Test]
        public void OneShot_AlreadyShown_IsExcluded()
        {
            var one = Card("tek"); one.OneShot = true;
            var pool = new List<EventCard> { Card("normal"), one };
            var state = new GameState(1);
            state.MarkCardShown("tek");

            var result = _svc.SelectNext(state, pool, new StubRandom(doubleResult: 0.0));

            Assert.AreEqual("normal", result.Value.Id);
        }

        [Test]
        public void Cooldown_RecentlyShown_IsExcluded()
        {
            var cd = Card("cd"); cd.CooldownTurns = 5;
            var pool = new List<EventCard> { Card("normal"), cd };
            var state = new GameState(1);
            for (int i = 0; i < 5; i++) state.AdvanceTurn(); // tur 5
            state.LastShownTurnByCard["cd"] = 3; // 5-3=2 < 5 → cooldown

            var result = _svc.SelectNext(state, pool, new StubRandom(doubleResult: 0.0));

            Assert.AreEqual("normal", result.Value.Id);
        }

        [Test]
        public void Emergency_OnlyAppears_DuringCrisis()
        {
            var emg = Card("acil"); emg.IsEmergency = true;
            var pool = new List<EventCard> { Card("normal"), emg };
            var state = new GameState(1);

            // Kriz yok → acil kart gelmez.
            Assert.AreEqual("normal", _svc.SelectNext(state, pool, new StubRandom(0, 0.0)).Value.Id);

            // Kriz var → acil kart öne geçer.
            state.StartCrisis("k1");
            Assert.AreEqual("acil", _svc.SelectNext(state, pool, new StubRandom(0, 0.0)).Value.Id);
        }

        [Test]
        public void LastShownCard_IsNotRepeatedImmediately()
        {
            var pool = new List<EventCard> { Card("a"), Card("b") };
            var state = new GameState(1);
            state.RecordDecision(new DecisionRecord("a", DecisionSide.Left, 0)); // en son 'a'

            var result = _svc.SelectNext(state, pool, new StubRandom(doubleResult: 0.0));

            Assert.AreEqual("b", result.Value.Id);
        }

        [Test]
        public void Selection_IsDeterministic_ForSameInputs()
        {
            var pool = new List<EventCard> { Card("a"), Card("b"), Card("c"), Card("d") };
            var s1 = new GameState(42);
            var s2 = new GameState(42);

            var r1 = _svc.SelectNext(s1, pool, new StubRandom(doubleResult: 0.62));
            var r2 = _svc.SelectNext(s2, pool, new StubRandom(doubleResult: 0.62));

            Assert.AreEqual(r1.Value.Id, r2.Value.Id);
        }

        [Test]
        public void Fallback_UsedWhenNoEligibleCard()
        {
            var blocked = Card("blocked");
            blocked.ValueConditions.Add(new ValueCondition(CountryValue.Economy, 90, 100)); // sağlanmaz
            var fb = Card("fallback"); fb.IsFallback = true;
            var pool = new List<EventCard> { blocked, fb };

            var result = _svc.SelectNext(new GameState(1), pool, new StubRandom());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("fallback", result.Value.Id);
        }

        [Test]
        public void Failure_WhenNoEligibleAndNoFallback()
        {
            var blocked = Card("blocked");
            blocked.ValueConditions.Add(new ValueCondition(CountryValue.Economy, 90, 100));
            var pool = new List<EventCard> { blocked };

            var result = _svc.SelectNext(new GameState(1), pool, new StubRandom());

            Assert.IsTrue(result.IsFailure);
        }

        [Test]
        public void EmptyPool_IsFailure()
        {
            var result = _svc.SelectNext(new GameState(1), new List<EventCard>(), new StubRandom());
            Assert.IsTrue(result.IsFailure);
        }
    }
}
