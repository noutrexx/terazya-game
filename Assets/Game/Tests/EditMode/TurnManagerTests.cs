using DengeGame.Application;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class TurnManagerTests
    {
        [Test]
        public void AdvanceTurn_RaisesEventWithNewTurn()
        {
            var manager = new TurnManager(new GameState(seed: 1));
            int reported = -1;
            manager.TurnAdvanced += t => reported = t;

            var result = manager.AdvanceTurn();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, reported);
            Assert.AreEqual(1, manager.State.CurrentTurn);
        }

        [Test]
        public void AdvanceTurn_FailsAfterGameEnded()
        {
            var manager = new TurnManager(new GameState(seed: 1));
            manager.EndGame("Test sonu");

            var result = manager.AdvanceTurn();

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(0, manager.State.CurrentTurn);
        }

        [Test]
        public void EndGame_RaisesEventOnce()
        {
            var manager = new TurnManager(new GameState(seed: 1));
            int calls = 0;
            manager.GameEnded += _ => calls++;

            manager.EndGame("Sebep");
            manager.EndGame("Tekrar"); // ikinci çağrı yok sayılmalı

            Assert.AreEqual(1, calls);
            Assert.AreEqual("Sebep", manager.State.EndingReason);
        }
    }
}
