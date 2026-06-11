using System;
using DengeGame.Application;
using DengeGame.Domain;

namespace DengeGame.Presentation
{
    /// <summary>
    /// IGameFlow implementasyonu. Oturum yaşam döngüsünü ve sahne geçişlerini koordine eder.
    /// Saf orkestrasyon; UI bilmez. Bootstrap tarafından oluşturulur.
    /// </summary>
    public sealed class GameFlow : IGameFlow
    {
        private readonly ISceneTransitionService _scenes;
        private readonly Func<int> _seedFactory;

        public GameSession CurrentSession { get; private set; }

        public GameFlow(ISceneTransitionService scenes, Func<int> seedFactory)
        {
            _scenes = scenes ?? throw new ArgumentNullException(nameof(scenes));
            _seedFactory = seedFactory ?? throw new ArgumentNullException(nameof(seedFactory));
        }

        public void StartNewGame(int? seed = null)
        {
            int actualSeed = seed ?? _seedFactory();
            var state = new GameState(actualSeed, new CountryStats());
            CurrentSession = new GameSession(state);
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
            _scenes.Load(GameScene.MainMenu);
        }
    }
}
