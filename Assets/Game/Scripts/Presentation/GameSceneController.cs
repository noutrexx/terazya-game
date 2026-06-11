using DengeGame.Application;
using UnityEngine;
using UnityEngine.UI;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Oyun sahnesinin kök kontrolcüsü (Faz 3 iskelet). Gerçek kart arayüzü Faz 6'da gelir;
    /// burada yalnızca tur ilerletme ve yönetimi bitirme akışı doğrulanır.
    /// </summary>
    public sealed class GameSceneController : MonoBehaviour, ISceneController
    {
        private GameServices _services;
        private Text _statusLabel;

        public void Construct(GameServices services) => _services = services;

        private void Start()
        {
            if (_services == null)
            {
                Debug.LogError("[Denge] GameSceneController servis enjeksiyonu yapılmadı.");
                return;
            }

            if (_services.Flow.CurrentSession == null)
            {
                Debug.LogError("[Denge] Aktif oturum yok. Oyun sahnesi yalnızca yeni oyun başlatıldıktan sonra açılmalı.");
                return;
            }

            var screen = SimpleUiFactory.CreateScreen("GameUI", new Color(0.13f, 0.15f, 0.20f));
            SimpleUiFactory.CreateLabel(screen, "Oyun Sahnesi (iskelet)", 56, Color.white);
            _statusLabel = SimpleUiFactory.CreateLabel(screen, "", 40, new Color(0.8f, 0.85f, 0.9f));
            SimpleUiFactory.CreateButton(screen, "Tur İlerlet", AdvanceTurn);
            SimpleUiFactory.CreateButton(screen, "Yönetimi Bitir", () =>
                _services.Flow.EndCurrentGame("Manuel bitiş (Faz 3 iskelet)"));

            RefreshStatus();
        }

        private void AdvanceTurn()
        {
            var result = _services.Flow.CurrentSession.Turns.AdvanceTurn();
            if (result.IsFailure)
                Debug.LogWarning($"[Denge] Tur ilerletilemedi: {result.Error}");
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            var state = _services.Flow.CurrentSession.State;
            _statusLabel.text =
                $"Tur: {state.CurrentTurn}\nYıl: {state.CurrentYear}\nDönem: {state.CurrentPeriod}\nSeed: {state.Seed}";
        }
    }
}
