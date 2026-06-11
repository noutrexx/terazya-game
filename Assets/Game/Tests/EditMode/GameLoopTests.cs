using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class GameLoopTests
    {
        private static GameLoop Loop(List<EventCard> pool, int seed = 1)
        {
            var session = new GameSession(new GameState(seed, new CountryStats()));
            return new GameLoop(session, pool,
                new WeightedEventSelectionService(),
                new DecisionEffectService(),
                new StubRandom(doubleResult: 0.0),
                new BoundaryEndingEvaluator());
        }

        private static EventCard Benign(string id, int leftDelta, int rightDelta)
        {
            var c = new EventCard(id, id);
            c.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Economy, leftDelta));
            c.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, rightDelta));
            return c;
        }

        [Test]
        public void Begin_PresentsACard()
        {
            var loop = Loop(new List<EventCard> { Benign("a", 2, -2) });
            var r = loop.Begin();
            Assert.IsTrue(r.IsSuccess);
            Assert.IsNotNull(loop.CurrentCard);
            Assert.IsTrue(loop.IsRunning);
        }

        [Test]
        public void Decide_AppliesEffects_AdvancesTurn_PresentsNext()
        {
            var loop = Loop(new List<EventCard> { Benign("a", 5, -5), Benign("b", 1, -1) });
            loop.Begin();
            var first = loop.CurrentCard.Id;

            var r = loop.Decide(DecisionSide.Left);

            Assert.IsTrue(r.IsSuccess);
            Assert.AreEqual(1, loop.Session.State.CurrentTurn);          // tur ilerledi
            Assert.AreEqual(1, loop.Session.State.DecisionHistory.Count); // karar kaydedildi
            Assert.IsNotNull(loop.CurrentCard);
            Assert.AreNotEqual(first, loop.CurrentCard.Id);              // ayni kart tekrar gelmedi
        }

        [Test]
        public void Decide_WhenNotRunning_Fails()
        {
            var loop = Loop(new List<EventCard> { Benign("a", 1, -1) });
            var r = loop.Decide(DecisionSide.Left); // Begin cagrilmadi
            Assert.IsTrue(r.IsFailure);
        }

        [Test]
        public void EndGameEffectCard_EndsLoop()
        {
            var ender = new EventCard("ender", "Son");
            ender.LeftEffects.Add(EffectData.EndGame("Özel Son"));
            ender.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 1));
            var loop = Loop(new List<EventCard> { ender });

            string endedReason = null;
            loop.Ended += r => endedReason = r;
            loop.Begin();
            loop.Decide(DecisionSide.Left);

            Assert.IsFalse(loop.IsRunning);
            Assert.IsTrue(loop.Session.State.IsEnded);
            Assert.AreEqual("Özel Son", endedReason);
        }

        [Test]
        public void BoundaryReached_EndsLoop_WithMatchingReason()
        {
            var crash = new EventCard("crash", "Çöküş");
            crash.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Economy, -100)); // 50 -> 0
            crash.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 1));
            var loop = Loop(new List<EventCard> { crash });

            loop.Begin();
            loop.Decide(DecisionSide.Left);

            Assert.IsTrue(loop.Session.State.IsEnded);
            Assert.AreEqual("Devlet İflası", loop.Session.State.EndingReason);
        }

        [Test]
        public void CurrentCard_IsClearedDuringDecide_PreventingDoubleApply()
        {
            var loop = Loop(new List<EventCard> { Benign("a", -100, -100) }); // ilk karar oyunu bitirir (eko 0)
            loop.Begin();
            loop.Decide(DecisionSide.Left);
            // Oyun bitti; ikinci karar reddedilmeli.
            var second = loop.Decide(DecisionSide.Left);
            Assert.IsTrue(second.IsFailure);
        }
    }
}
