using DengeGame.Domain;

namespace DengeGame.Application.Endings
{
    /// <summary>
    /// Birden çok son değerlendiriciyi sırayla dener; ilk son veren sonucu döndürür.
    /// Sıra öncelik belirler (örn. önce sınır sonları, sonra karakter sonları).
    /// </summary>
    public sealed class CompositeEndingEvaluator : IEndingEvaluator
    {
        private readonly IEndingEvaluator[] _evaluators;

        public CompositeEndingEvaluator(params IEndingEvaluator[] evaluators)
        {
            _evaluators = evaluators ?? new IEndingEvaluator[0];
        }

        public string Evaluate(GameState state)
        {
            foreach (var e in _evaluators)
            {
                var reason = e?.Evaluate(state);
                if (!string.IsNullOrEmpty(reason)) return reason;
            }
            return null;
        }
    }
}
