using System;
using DengeGame.Domain;

namespace DengeGame.Application.Input
{
    public enum SwipeSide { None = 0, Left = 1, Right = 2 }

    /// <summary>Bir sürükleme anının değerlendirilmiş durumu (UI bundan beslenir).</summary>
    public readonly struct SwipeState
    {
        /// <summary>-1..1 arası ilerleme (negatif: sola, pozitif: sağa).</summary>
        public readonly float Progress;
        /// <summary>Kartın derece cinsinden dönüşü.</summary>
        public readonly float Rotation;
        /// <summary>Eşik geçildiyse taahhüt edilen taraf, yoksa None.</summary>
        public readonly SwipeSide CommittedSide;

        public bool IsCommitted => CommittedSide != SwipeSide.None;

        public SwipeState(float progress, float rotation, SwipeSide side)
        {
            Progress = progress;
            Rotation = rotation;
            CommittedSide = side;
        }

        public DecisionSide ToDecisionSide() =>
            CommittedSide == SwipeSide.Left ? DecisionSide.Left : DecisionSide.Right;
    }

    /// <summary>
    /// Kaydırma kararı matematiği (saf C#, Unity'siz → EditMode'da test edilebilir).
    /// Sürükleme miktarını ilerleme, dönüş ve taahhüt tarafına çevirir.
    /// </summary>
    public static class SwipeEvaluator
    {
        public static SwipeState Evaluate(float dragDeltaX, float threshold, float maxRotation)
        {
            float t = threshold <= 0f ? 1f : threshold;
            float progress = Clamp(dragDeltaX / t, -1f, 1f);
            float rotation = progress * maxRotation; // sağa sürükle → sağa eğil

            SwipeSide side = SwipeSide.None;
            if (dragDeltaX <= -t) side = SwipeSide.Left;
            else if (dragDeltaX >= t) side = SwipeSide.Right;

            return new SwipeState(progress, rotation, side);
        }

        private static float Clamp(float v, float min, float max) =>
            v < min ? min : (v > max ? max : v);
    }
}
