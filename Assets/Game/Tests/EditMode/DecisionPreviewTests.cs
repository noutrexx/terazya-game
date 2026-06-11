using DengeGame.Application;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class DecisionPreviewTests
    {
        [Test]
        public void Preview_ReportsDirection_NotMagnitude()
        {
            var card = new EventCard("x", "X");
            card.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 25));
            card.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Freedom, -8));

            var hints = DecisionPreview.Preview(card, DecisionSide.Left);

            Assert.AreEqual(HintDirection.Up, hints[CountryValue.Economy]);
            Assert.AreEqual(HintDirection.Down, hints[CountryValue.Freedom]);
            Assert.IsFalse(hints.ContainsKey(CountryValue.Security)); // etkilenmeyen yok
        }

        [Test]
        public void Preview_NetsOutOpposingEffects()
        {
            var card = new EventCard("x", "X");
            card.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 10));
            card.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, -10)); // net 0

            var hints = DecisionPreview.Preview(card, DecisionSide.Right);

            Assert.IsFalse(hints.ContainsKey(CountryValue.Economy));
        }

        [Test]
        public void Preview_IncludesRandomAndTimedEffects()
        {
            var card = new EventCard("x", "X");
            card.LeftEffects.Add(EffectData.RandomValue(CountryValue.Security, 2, 6)); // pozitif
            card.LeftEffects.Add(EffectData.TimedValue(CountryValue.Environment, -3, 4)); // negatif

            var hints = DecisionPreview.Preview(card, DecisionSide.Left);

            Assert.AreEqual(HintDirection.Up, hints[CountryValue.Security]);
            Assert.AreEqual(HintDirection.Down, hints[CountryValue.Environment]);
        }
    }
}
