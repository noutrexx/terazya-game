using DengeGame.Application.Endings;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class BoundaryEndingEvaluatorTests
    {
        private readonly BoundaryEndingEvaluator _eval = new BoundaryEndingEvaluator();

        [Test]
        public void NoBoundary_ReturnsNull()
        {
            Assert.IsNull(_eval.Evaluate(new GameState(1, new CountryStats())));
        }

        [Test]
        public void EconomyZero_IsBankruptcy()
        {
            var s = new GameState(1, new CountryStats());
            s.Stats.Set(CountryValue.Economy, 0);
            Assert.AreEqual("Devlet İflası", _eval.Evaluate(s));
        }

        [Test]
        public void SecurityFull_IsOppressiveRegime()
        {
            var s = new GameState(1, new CountryStats());
            s.Stats.Set(CountryValue.Security, 100);
            Assert.AreEqual("Baskıcı Güvenlik Rejimi", _eval.Evaluate(s));
        }

        [Test]
        public void FreedomZero_IsAuthoritarian()
        {
            var s = new GameState(1, new CountryStats());
            s.Stats.Set(CountryValue.Freedom, 0);
            Assert.AreEqual("Otoriter Rejim", _eval.Evaluate(s));
        }
    }
}
