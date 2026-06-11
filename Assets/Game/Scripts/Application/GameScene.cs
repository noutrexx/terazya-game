namespace DengeGame.Application
{
    /// <summary>
    /// Oyundaki sahneler. Sahne adları tek bir yerde tanımlanır; sihirli string'ler önlenir.
    /// </summary>
    public enum GameScene
    {
        MainMenu = 0,
        Game = 1,
        Result = 2
    }

    public static class GameSceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string Game = "Game";
        public const string Result = "Result";

        public static string ToSceneName(this GameScene scene)
        {
            switch (scene)
            {
                case GameScene.MainMenu: return MainMenu;
                case GameScene.Game: return Game;
                case GameScene.Result: return Result;
                default: return MainMenu;
            }
        }
    }

    /// <summary>
    /// Sahne geçişi soyutlaması (port). Unity SceneManager implementasyonda gizlenir.
    /// </summary>
    public interface ISceneTransitionService
    {
        void Load(GameScene scene);
    }
}
