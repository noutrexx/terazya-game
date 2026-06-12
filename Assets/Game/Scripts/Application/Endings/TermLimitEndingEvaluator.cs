using DengeGame.Domain;

namespace DengeGame.Application.Endings
{
    /// <summary>
    /// Maksimum görev süresi tamamlanınca olumlu bir son tetikler (örn. Onurlu Emeklilik).
    /// </summary>
    public sealed class TermLimitEndingEvaluator : IEndingEvaluator
    {
        private readonly int _maxTurns;
        private readonly string _reason;

        public TermLimitEndingEvaluator(int maxTurns, string reason = "Onurlu Emeklilik")
        {
            _maxTurns = maxTurns;
            _reason = reason;
        }

        public string Evaluate(GameState state)
        {
            if (state == null || state.IsEnded) return null;
            return state.CurrentTurn >= _maxTurns ? _reason : null;
        }
    }
}
