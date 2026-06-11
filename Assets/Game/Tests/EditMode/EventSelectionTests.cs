using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    /// <summary>
    /// Faz 3 iskelet seçim servisi testleri. Asıl odak: DETERMİNİZM (aynı seed → aynı seçim).
    /// Faz 5'te tam algoritma testleriyle genişletilecektir.
    /// </summary>
    public sealed class EventSelectionTests
    {
        private static List<EventCard> Pool() => new List<EventCard>
        {
            new EventCard("card_a", "A") { MinTurn = 0 },
            new EventCard("card_b", "B") { MinTurn = 0 },
            new EventCard("card_c", "C") { MinTurn = 5 },
            new EventCard("card_d", "D") { OneShot = true }
        };

        // Test için sahte, deterministik rastgelelik (sabit dizi).
        private sealed class FakeRandom : IRandomService
        {
            private readonly int _fixed;
            public FakeRandom(int fixedValue) => _fixed = fixedValue;
            public void Reseed(int seed) { }
            public int NextInt(int maxExclusive) => _fixed % (maxExclusive <= 0 ? 1 : maxExclusive);
            public int NextInt(int min, int max) => min;
            public double NextDouble() => 0.0;
        }

        [Test]
        public void Select_FiltersByMinTurn()
        {
            var service = new BasicEventSelectionService();
            var state = new GameState(seed: 1); // tur 0 → card_c (MinTurn 5) elenmeli
            var result = service.SelectNext(state, Pool(), new FakeRandom(0));
            Assert.IsTrue(result.IsSuccess);
            Assert.AreNotEqual("card_c", result.Value.Id);
        }

        [Test]
        public void Select_SkipsShownOneShotCards()
        {
            var service = new BasicEventSelectionService();
            var state = new GameState(seed: 1);
            state.MarkCardShown("card_d");

            // Sıralı (a,b,d) → d elenince (a,b); index 2 % 2 = 0 → "card_a"
            var result = service.SelectNext(state, Pool(), new FakeRandom(2));
            Assert.IsTrue(result.IsSuccess);
            Assert.AreNotEqual("card_d", result.Value.Id);
        }

        [Test]
        public void Select_IsDeterministic_ForSameInputs()
        {
            var service = new BasicEventSelectionService();
            var state = new GameState(seed: 1);

            var r1 = service.SelectNext(state, Pool(), new FakeRandom(1));
            var r2 = service.SelectNext(state, Pool(), new FakeRandom(1));

            Assert.IsTrue(r1.IsSuccess && r2.IsSuccess);
            Assert.AreEqual(r1.Value.Id, r2.Value.Id);
        }

        [Test]
        public void Select_FailsOnEmptyPool()
        {
            var service = new BasicEventSelectionService();
            var result = service.SelectNext(new GameState(1), new List<EventCard>(), new FakeRandom(0));
            Assert.IsTrue(result.IsFailure);
        }
    }
}
