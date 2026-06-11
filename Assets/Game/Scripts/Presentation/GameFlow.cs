using System;
using System.Collections.Generic;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Domain;

namespace DengeGame.Presentation
{
    /// <summary>
    /// IGameFlow implementasyonu. Oturum + oyun döngüsü yaşam döngüsünü ve sahne geçişlerini koordine eder.
    /// GameLoop'u, enjekte edilen servislerle ve kart havuzu sağlayıcısıyla kurar.
    /// </summary>
    public sealed class GameFlow : IGameFlow
    {
        private readonly ISceneTransitionService _scenes;
        private readonly Func<int> _seedFactory;
        private readonly Func<List<EventCard>> _poolProvider;
        private readonly IEventSelectionService _selection;
        private readonly IDecisionEffectService _effects;
        private readonly IRandomService _random;
        private readonly IEndingEvaluator _endings;

        public GameSession CurrentSession { get; private set; }
        public GameLoop CurrentLoop { get; private set; }

        public GameFlow(ISceneTransitionService scenes, Func<int> seedFactory,
            Func<List<EventCard>> poolProvider, IEventSelectionService selection,
            IDecisionEffectService effects, IRandomService random, IEndingEvaluator endings)
        {
            _scenes = scenes ?? throw new ArgumentNullException(nameof(scenes));
            _seedFactory = seedFactory ?? throw new ArgumentNullException(nameof(seedFactory));
            _poolProvider = poolProvider ?? throw new ArgumentNullException(nameof(poolProvider));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _endings = endings ?? throw new ArgumentNullException(nameof(endings));
        }

        public void StartNewGame(int? seed = null)
        {
            int actualSeed = seed ?? _seedFactory();
            _random.Reseed(actualSeed); // determinizm: her oyun kendi seed'iyle

            var state = new GameState(actualSeed, new CountryStats());
            CurrentSession = new GameSession(state);

            var pool = _poolProvider() ?? new List<EventCard>();
            CurrentLoop = new GameLoop(CurrentSession, pool, _selection, _effects, _random, _endings);

            _scenes.Load(GameScene.Game);
        }

        public void EndCurrentGame(string reason)
        {
            CurrentSession?.Turns.EndGame(reason);
            _scenes.Load(GameScene.Result);
        }

        public void ReturnToMenu()
        {
            CurrentSession = null;
            CurrentLoop = null;
            _scenes.Load(GameScene.MainMenu);
        }
    }
}
