using UnityEngine;

namespace DengeGame.Presentation
{
    /// <summary>Ana menü sahnesinin kök kontrolcüsü (Faz 3 iskelet).</summary>
    public sealed class MainMenuController : MonoBehaviour, ISceneController
    {
        private GameServices _services;

        public void Construct(GameServices services) => _services = services;

        private void Start()
        {
            if (_services == null)
            {
                Debug.LogError("[Denge] MainMenuController servis enjeksiyonu yapılmadı. " +
                               "Bootstrap sahnede çalışıyor mu? (GameEntryPoint otomatik oluşturur.)");
                return;
            }

            var screen = SimpleUiFactory.CreateScreen("MainMenuUI", new Color(0.10f, 0.12f, 0.16f));
            SimpleUiFactory.CreateLabel(screen, "DENGE", 96, Color.white);
            SimpleUiFactory.CreateLabel(screen, "Bir Ülkenin Hikâyesi", 40, new Color(0.7f, 0.75f, 0.85f));
            SimpleUiFactory.CreateButton(screen, "Yeni Yönetim", () => _services.Flow.StartNewGame());
        }
    }
}
