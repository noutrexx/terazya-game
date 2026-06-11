using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class CountryStatsTests
    {
        [Test]
        public void DefaultConstructor_SetsAllValuesTo50()
        {
            var stats = new CountryStats();
            foreach (var value in CountryValueInfo.All)
                Assert.AreEqual(50, stats.Get(value));
        }

        [Test]
        public void Apply_ClampsToUpperBound()
        {
            var stats = new CountryStats(90);
            stats.Apply(CountryValue.Economy, 50);
            Assert.AreEqual(100, stats.Get(CountryValue.Economy));
        }

        [Test]
        public void Apply_ClampsToLowerBound()
        {
            var stats = new CountryStats(10);
            stats.Apply(CountryValue.Security, -50);
            Assert.AreEqual(0, stats.Get(CountryValue.Security));
        }

        [Test]
        public void Apply_ReturnsActualNetChange_WhenClamped()
        {
            var stats = new CountryStats(95);
            int net = stats.Apply(CountryValue.Economy, 20); // 95 -> 100
            Assert.AreEqual(5, net);
        }

        [Test]
        public void Set_OverwritesValueWithinBounds()
        {
            var stats = new CountryStats();
            int previous = stats.Set(CountryValue.Freedom, 73);
            Assert.AreEqual(50, previous);
            Assert.AreEqual(73, stats.Get(CountryValue.Freedom));
        }

        [Test]
        public void CriticalFlags_DetectBounds()
        {
            var low = new CountryStats(0);
            var high = new CountryStats(100);
            Assert.IsTrue(low.IsAtCriticalLow(CountryValue.Diplomacy));
            Assert.IsFalse(low.IsAtCriticalHigh(CountryValue.Diplomacy));
            Assert.IsTrue(high.IsAtCriticalHigh(CountryValue.Environment));
            Assert.IsFalse(high.IsAtCriticalLow(CountryValue.Environment));
        }

        [Test]
        public void Clone_IsIndependentCopy()
        {
            var stats = new CountryStats(40);
            var clone = stats.Clone();
            clone.Set(CountryValue.Economy, 99);
            Assert.AreEqual(40, stats.Get(CountryValue.Economy));
            Assert.AreEqual(99, clone.Get(CountryValue.Economy));
        }
    }
}
