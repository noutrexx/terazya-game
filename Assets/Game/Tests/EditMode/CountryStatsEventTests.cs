using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class CountryStatsEventTests
    {
        [Test]
        public void Changed_FiresWithReasonAndDelta_OnApply()
        {
            var stats = new CountryStats();
            CountryStatChange captured = default;
            int callCount = 0;
            stats.Changed += c => { captured = c; callCount++; };

            stats.Apply(CountryValue.Economy, -8, "vergi reformu");

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(CountryValue.Economy, captured.Value);
            Assert.AreEqual(50, captured.Previous);
            Assert.AreEqual(42, captured.Current);
            Assert.AreEqual(-8, captured.Delta);
            Assert.AreEqual("vergi reformu", captured.Reason);
        }

        [Test]
        public void Changed_DoesNotFire_WhenClampPreventsChange()
        {
            var stats = new CountryStats(100);
            int callCount = 0;
            stats.Changed += _ => callCount++;

            stats.Apply(CountryValue.Economy, +10); // zaten 100, değişmez

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Changed_FiresOnSet_WhenValueChanges()
        {
            var stats = new CountryStats();
            int callCount = 0;
            stats.Changed += _ => callCount++;

            stats.Set(CountryValue.Freedom, 30, "olağanüstü hal");
            stats.Set(CountryValue.Freedom, 30); // aynı değer, event yok

            Assert.AreEqual(1, callCount);
        }
    }
}
