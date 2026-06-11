using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Application.Endings;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class CharacterSystemTests
    {
        // --- İlişki durumu etiketleri ---

        [Test]
        public void RelationshipStatus_MapsDefaultTiers()
        {
            Assert.AreEqual("Düşman", RelationshipStatus.Of(-100));
            Assert.AreEqual("Mesafeli", RelationshipStatus.Of(-40));
            Assert.AreEqual("Tarafsız", RelationshipStatus.Of(0));
            Assert.AreEqual("Müttefik", RelationshipStatus.Of(40));
            Assert.AreEqual("Sadık", RelationshipStatus.Of(80));
        }

        [Test]
        public void RelationshipStatus_UsesCustomTiers_WhenProvided()
        {
            var tiers = new[] { "A", "B", "C", "D", "E" };
            Assert.AreEqual("C", RelationshipStatus.Of(0, tiers));
            Assert.AreEqual("E", RelationshipStatus.Of(100, tiers));
        }

        // --- İlişki geçmişi ---

        [Test]
        public void ApplyRelationship_RecordsHistory_OnChange()
        {
            var state = new GameState(1);
            state.AdvanceTurn(); // tur 1
            state.ApplyRelationship("general", -30);

            Assert.AreEqual(1, state.RelationshipHistory.Count);
            var h = state.RelationshipHistory[0];
            Assert.AreEqual("general", h.CharacterId);
            Assert.AreEqual(-30, h.Delta);
            Assert.AreEqual(-30, h.ResultingValue);
            Assert.AreEqual(1, h.Turn);
        }

        [Test]
        public void ApplyRelationship_NoHistory_WhenClampedToNoChange()
        {
            var state = new GameState(1);
            state.ApplyRelationship("x", -100); // -100
            state.RelationshipHistory.Clear();
            state.ApplyRelationship("x", -50);  // zaten -100, değişmez
            Assert.AreEqual(0, state.RelationshipHistory.Count);
        }

        // --- Karakter sonu ---

        private static Character Coup() => new Character("genelkurmay", "K")
        {
            EnableLowEnding = true,
            LowEndingThreshold = -80,
            LowEndingReason = "Askeri Darbe"
        };

        [Test]
        public void CharacterEnding_Triggers_WhenRelationshipBelowThreshold()
        {
            var state = new GameState(1);
            state.ApplyRelationship("genelkurmay", -85);
            var eval = new CharacterEndingEvaluator(new[] { Coup() });
            Assert.AreEqual("Askeri Darbe", eval.Evaluate(state));
        }

        [Test]
        public void CharacterEnding_DoesNotTrigger_AboveThreshold()
        {
            var state = new GameState(1);
            state.ApplyRelationship("genelkurmay", -50);
            var eval = new CharacterEndingEvaluator(new[] { Coup() });
            Assert.IsNull(eval.Evaluate(state));
        }

        // --- Bileşik son ---

        [Test]
        public void Composite_ReturnsFirstNonNull_InOrder()
        {
            var state = new GameState(1, new CountryStats());
            state.Stats.Set(CountryValue.Economy, 0);      // sınır sonu
            state.ApplyRelationship("genelkurmay", -90);   // karakter sonu

            var composite = new CompositeEndingEvaluator(
                new BoundaryEndingEvaluator(),
                new CharacterEndingEvaluator(new[] { Coup() }));

            Assert.AreEqual("Devlet İflası", composite.Evaluate(state)); // sınır önce
        }

        [Test]
        public void Composite_FallsThroughToCharacterEnding()
        {
            var state = new GameState(1, new CountryStats());
            state.ApplyRelationship("genelkurmay", -90);

            var composite = new CompositeEndingEvaluator(
                new BoundaryEndingEvaluator(),
                new CharacterEndingEvaluator(new[] { Coup() }));

            Assert.AreEqual("Askeri Darbe", composite.Evaluate(state));
        }

        // --- Dizin ---

        [Test]
        public void Directory_LooksUpById_AndFallsBackToId()
        {
            var dir = new CharacterDirectory(new List<Character>
            {
                new Character("maliye", "Elif Demir") { Role = "Maliye Bakanı" }
            });
            Assert.AreEqual("Elif Demir", dir.Get("maliye").Name);
            Assert.IsNull(dir.Get("yok"));
            Assert.AreEqual("yok", dir.DisplayName("yok"));
        }
    }
}
