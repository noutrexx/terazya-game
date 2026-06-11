using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class GameStateTests
    {
        [Test]
        public void NewState_HasExpectedDefaults()
        {
            var state = new GameState(seed: 123);
            Assert.AreEqual(0, state.CurrentTurn);
            Assert.AreEqual(1, state.CurrentYear);
            Assert.AreEqual(GamePeriod.Founding, state.CurrentPeriod);
            Assert.IsFalse(state.IsEnded);
            Assert.AreEqual(123, state.Seed);
        }

        [Test]
        public void AdvanceTurn_IncrementsTurnAndYear()
        {
            var state = new GameState(seed: 1, turnsPerYear: 4);
            for (int i = 0; i < 4; i++) state.AdvanceTurn();
            Assert.AreEqual(4, state.CurrentTurn);
            Assert.AreEqual(2, state.CurrentYear); // 1 + 4/4
        }

        [Test]
        public void AdvanceTurn_TransitionsPeriods()
        {
            var state = new GameState(seed: 1, turnsPerYear: 4);
            // Yıl 9'a kadar ilerlet (Growth biter, Maturity başlar: yıl > 8)
            for (int i = 0; i < 36; i++) state.AdvanceTurn(); // tur 36 -> yıl 10
            Assert.AreEqual(GamePeriod.Maturity, state.CurrentPeriod);
        }

        [Test]
        public void MarkCardShown_RecordsTurnAndId()
        {
            var state = new GameState(seed: 1);
            state.AdvanceTurn();
            state.MarkCardShown("card_a");
            Assert.IsTrue(state.ShownCardIds.Contains("card_a"));
            Assert.AreEqual(1, state.LastShownTurnByCard["card_a"]);
        }

        [Test]
        public void EndGame_SetsEndedAndReason()
        {
            var state = new GameState(seed: 1);
            state.EndGame("Devlet İflası");
            Assert.IsTrue(state.IsEnded);
            Assert.AreEqual("Devlet İflası", state.EndingReason);
        }
    }
}
