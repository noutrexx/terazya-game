using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Domain;
using NUnit.Framework;
using E = DengeGame.Domain.EffectData;
using V = DengeGame.Domain.CountryValue;

namespace DengeGame.Tests.EditMode
{
    public sealed class ScoringEndingTests
    {
        // --- Skor ---

        [Test]
        public void Score_CombinesAllComponents()
        {
            var state = new GameState(1, new CountryStats()); // tüm değerler 50
            for (int i = 0; i < 10; i++) state.AdvanceTurn();  // tur 10

            state.StartCrisis("a"); state.ResolveCrisis("a");
            state.StartCrisis("b"); state.ResolveCrisis("b");
            state.StartCrisis("c"); state.FailCrisis("c");
            state.IncrementCompletedChains();
            state.IncrementCompletedChains();
            state.IncrementCompletedChains();

            var s = ScoreCalculator.Calculate(state, EndingRarity.Rare);

            Assert.AreEqual(100, s.TurnScore);
            Assert.AreEqual(300, s.BalanceScore);        // 6 * 50
            Assert.AreEqual(85, s.CrisisScore);          // 2*50 - 1*15
            Assert.AreEqual(120, s.ChainScore);          // 3*40
            Assert.AreEqual(150, s.EndingBonus);         // Rare
            Assert.AreEqual(755, s.Total);
        }

        [Test]
        public void Score_BalanceDropsWithExtremeValues()
        {
            var state = new GameState(1, new CountryStats());
            state.Stats.Set(V.Economy, 100); // uçta → bu değer için denge 0
            var s = ScoreCalculator.Calculate(state);
            Assert.AreEqual(250, s.BalanceScore); // 5*50 + 0
        }

        // --- Süre sonu ---

        [Test]
        public void TermLimit_TriggersAtMaxTurns()
        {
            var eval = new TermLimitEndingEvaluator(5, "Onurlu Emeklilik");
            var state = new GameState(1);
            Assert.IsNull(eval.Evaluate(state));
            for (int i = 0; i < 5; i++) state.AdvanceTurn();
            Assert.AreEqual("Onurlu Emeklilik", eval.Evaluate(state));
        }

        // --- Son kaydı ---

        [Test]
        public void EndingRegistry_ResolvesKnown_AndDefaultsUnknown()
        {
            var reg = new EndingRegistry(new[]
            {
                new EndingDefinition("Devlet İflası", "Devlet İflası")
                    { Category = EndingCategory.Disaster, Rarity = EndingRarity.Common }
            });
            Assert.AreEqual(EndingCategory.Disaster, reg.GetOrDefault("Devlet İflası").Category);
            var def = reg.GetOrDefault("Tanımsız Son");
            Assert.AreEqual("Tanımsız Son", def.Title);
            Assert.AreEqual(EndingCategory.Neutral, def.Category);
        }

        // --- Kriz sayaçları ---

        [Test]
        public void ResolveAndFailCrisis_TrackSeparately()
        {
            var state = new GameState(1);
            state.StartCrisis("x"); state.ResolveCrisis("x");
            state.StartCrisis("y"); state.FailCrisis("y");
            Assert.AreEqual(1, state.ResolvedCrisisCount);
            Assert.AreEqual(1, state.FailedCrisisCount);
        }

        [Test]
        public void EndCrisisEffect_CountsAsResolved()
        {
            var state = new GameState(1);
            state.StartCrisis("k");
            var fx = EffectFactory.CreateMany(new List<E> { E.EndCrisis("k") });
            new DecisionEffectService().ApplyEffects(state, fx, new StubRandom());
            Assert.AreEqual(1, state.ResolvedCrisisCount);
        }

        // --- Zincir tamamlama (GameLoop) ---

        [Test]
        public void Chain_FinalDecision_IncrementsCompletedChains()
        {
            var a = new EventCard("a", "A");
            a.LeftEffects.Add(E.ScheduleEvent("b", 1));
            a.RightEffects.Add(E.ChangeValue(V.Economy, 1));
            var b = new EventCard("b", "B") { PreviousEventId = "a" };
            b.LeftEffects.Add(E.ChangeValue(V.Economy, 1));   // sonrasını zamanlamaz → zincir biter
            b.RightEffects.Add(E.ChangeValue(V.Economy, -1));

            var session = new GameSession(new GameState(1, new CountryStats()));
            var loop = new GameLoop(session, new List<EventCard> { a, b },
                new WeightedEventSelectionService(), new DecisionEffectService(),
                new StubRandom(doubleResult: 0.0), new BoundaryEndingEvaluator());

            loop.Begin();              // A
            loop.Decide(DecisionSide.Left); // -> B
            loop.Decide(DecisionSide.Left); // B final
            Assert.AreEqual(1, session.State.CompletedChainCount);
        }
    }
}
