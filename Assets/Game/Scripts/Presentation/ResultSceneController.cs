using UnityEngine;

namespace DengeGame.Presentation
{
    /// <summary>Sonuç sahnesinin kök kontrolcüsü (Faz 3 iskelet). Tam sonuç ekranı Faz 9'da gelir.</summary>
    public sealed class ResultSceneController : MonoBehaviour, ISceneController
    {
        private GameServices _services;

        public void Construct(GameServices services) => _services = services;

        private void Start()
        {
            if (_services == null)
            {
                Debug.LogError("[Denge] ResultSceneController servis enjeksiyonu yapılmadı.");
                return;
            }

            var screen = SimpleUiFactory.CreateScreen("ResultUI", new Color(0.16f, 0.12f, 0.14f));
            SimpleUiFactory.CreateLabel(screen, "Yönetim Sona Erdi", 64, Color.white);

            var session = _services.Flow.CurrentSession;
            if (session != null)
            {
                var state = session.State;
                SimpleUiFactory.CreateLabel(screen,
                    $"Sebep: {state.EndingReason}\nGörevde kalınan tur: {state.CurrentTurn}",
                    40, new Color(0.85f, 0.8f, 0.8f));
            }

            SimpleUiFactory.CreateButton(screen, "Ana Menü", () => _services.Flow.ReturnToMenu());
        }
    }
}
