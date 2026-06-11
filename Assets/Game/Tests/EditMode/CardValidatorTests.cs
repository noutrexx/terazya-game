using System.Collections.Generic;
using System.Linq;
using DengeGame.Application.Validation;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class CardValidatorTests
    {
        private static EventCard Valid(string id)
        {
            var c = new EventCard(id, "Başlık")
            {
                CharacterId = "x",
                Description = "Bir açıklama.",
                LeftText = "Sol",
                RightText = "Sağ"
            };
            c.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 3));
            c.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, -3));
            return c;
        }

        private static bool HasError(List<CardIssue> issues, string contains) =>
            issues.Any(i => i.Severity == IssueSeverity.Error && i.Message.Contains(contains));

        [Test]
        public void ValidCards_ProduceNoErrors()
        {
            var issues = CardValidator.Validate(new List<EventCard> { Valid("a"), Valid("b") });
            Assert.AreEqual(0, issues.Count(i => i.Severity == IssueSeverity.Error));
        }

        [Test]
        public void DuplicateId_IsError()
        {
            var issues = CardValidator.Validate(new List<EventCard> { Valid("dup"), Valid("dup") });
            Assert.IsTrue(HasError(issues, "Tekrarlanan ID"));
        }

        [Test]
        public void MissingText_IsError()
        {
            var c = Valid("x"); c.Description = "";
            var issues = CardValidator.Validate(new List<EventCard> { c });
            Assert.IsTrue(HasError(issues, "Açıklama boş"));
        }

        [Test]
        public void InvalidTurnRange_IsError()
        {
            var c = Valid("x"); c.MinTurn = 10; c.MaxTurn = 5;
            var issues = CardValidator.Validate(new List<EventCard> { c });
            Assert.IsTrue(HasError(issues, "MinTurn"));
        }

        [Test]
        public void InvalidValueConditionRange_IsError()
        {
            var c = Valid("x");
            c.ValueConditions.Add(new ValueCondition(CountryValue.Security, 80, 20));
            var issues = CardValidator.Validate(new List<EventCard> { c });
            Assert.IsTrue(HasError(issues, "min"));
        }

        [Test]
        public void BrokenChain_MissingPrevious_IsError()
        {
            var c = Valid("devam"); c.PreviousEventId = "yok";
            var issues = CardValidator.Validate(new List<EventCard> { c });
            Assert.IsTrue(HasError(issues, "önceki olay"));
        }

        [Test]
        public void BrokenChain_ScheduleTargetMissing_IsError()
        {
            var c = Valid("basla");
            c.LeftEffects.Add(EffectData.ScheduleEvent("hayalet", 2));
            var issues = CardValidator.Validate(new List<EventCard> { c });
            Assert.IsTrue(HasError(issues, "zamanlanan olay"));
        }

        [Test]
        public void ReachableChain_ProducesNoChainError()
        {
            var start = Valid("basla");
            start.LeftEffects.Add(EffectData.ScheduleEvent("devam", 2));
            var cont = Valid("devam");
            cont.PreviousEventId = "basla";

            var issues = CardValidator.Validate(new List<EventCard> { start, cont });

            Assert.IsFalse(HasError(issues, "önceki olay"));
            Assert.IsFalse(HasError(issues, "zamanlanan olay"));
        }

        [Test]
        public void IdenticalDecisions_IsWarning()
        {
            var c = Valid("x");
            c.LeftText = "Aynı"; c.RightText = "Aynı";
            c.LeftEffects = new List<EffectData> { EffectData.ChangeValue(CountryValue.Economy, 5) };
            c.RightEffects = new List<EffectData> { EffectData.ChangeValue(CountryValue.Economy, 5) };
            var issues = CardValidator.Validate(new List<EventCard> { c });
            Assert.IsTrue(issues.Any(i => i.Severity == IssueSeverity.Warning && i.Message.Contains("aynı")));
        }
    }
}
