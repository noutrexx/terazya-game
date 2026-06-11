using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>
    /// Aktif yönetimin (oyunun) oturumunu tutar: GameState + TurnManager.
    /// Saf C#; sahne/UI'dan bağımsızdır. Yeni oyun başlatma composition root tarafından yapılır.
    /// </summary>
    public sealed class GameSession
    {
        public GameState State { get; }
        public TurnManager Turns { get; }

        public GameSession(GameState state)
        {
            State = Guard.NotNull(state, nameof(state));
            Turns = new TurnManager(state);
        }
    }
}
