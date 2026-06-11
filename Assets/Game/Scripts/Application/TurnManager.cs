using System;
using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>
    /// Tur akışını yöneten saf C# orkestratörü. MonoBehaviour değildir; test edilebilir.
    /// Faz 4'te karar/etki uygulama servisiyle bütünleşecektir.
    /// </summary>
    public sealed class TurnManager
    {
        private readonly GameState _state;

        /// <summary>Tur ilerlediğinde yayınlanır (yeni tur numarası).</summary>
        public event Action<int> TurnAdvanced;

        /// <summary>Yönetim sona erdiğinde yayınlanır (sebep).</summary>
        public event Action<string> GameEnded;

        public TurnManager(GameState state)
        {
            _state = Guard.NotNull(state, nameof(state));
        }

        public GameState State => _state;

        /// <summary>
        /// Turu bir ilerletir. Oyun zaten bittiyse hata döndürür.
        /// </summary>
        public Result AdvanceTurn()
        {
            if (_state.IsEnded)
                return Result.Failure("Yönetim sona erdi; tur ilerletilemez.");

            _state.AdvanceTurn();
            ProcessTimedEffects();
            TurnAdvanced?.Invoke(_state.CurrentTurn);
            return Result.Success();
        }

        /// <summary>Yönetimi belirtilen sebeple sonlandırır.</summary>
        public void EndGame(string reason)
        {
            if (_state.IsEnded) return;
            _state.EndGame(reason);
            GameEnded?.Invoke(reason);
        }

        /// <summary>
        /// Aktif süreli etkileri işler: her birinin değerini uygular, kalan turu azaltır,
        /// süresi dolanları kaldırır. Listeyi geriye doğru gezerek güvenli silme yapar.
        /// </summary>
        private void ProcessTimedEffects()
        {
            var effects = _state.ActiveTimedEffects;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var effect = effects[i];
                _state.Stats.Apply(effect.Value, effect.PerTurnDelta, effect.Reason);
                effect.RemainingTurns--;
                if (effect.RemainingTurns <= 0)
                    effects.RemoveAt(i);
            }
        }
    }
}
