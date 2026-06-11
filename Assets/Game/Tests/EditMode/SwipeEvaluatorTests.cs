using DengeGame.Application.Input;
using DengeGame.Domain;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class SwipeEvaluatorTests
    {
        [Test]
        public void BelowThreshold_IsNotCommitted()
        {
            var s = SwipeEvaluator.Evaluate(dragDeltaX: 40f, threshold: 100f, maxRotation: 15f);
            Assert.AreEqual(SwipeSide.None, s.CommittedSide);
            Assert.IsFalse(s.IsCommitted);
        }

        [Test]
        public void RightBeyondThreshold_CommitsRight()
        {
            var s = SwipeEvaluator.Evaluate(120f, 100f, 15f);
            Assert.AreEqual(SwipeSide.Right, s.CommittedSide);
            Assert.AreEqual(DecisionSide.Right, s.ToDecisionSide());
        }

        [Test]
        public void LeftBeyondThreshold_CommitsLeft()
        {
            var s = SwipeEvaluator.Evaluate(-120f, 100f, 15f);
            Assert.AreEqual(SwipeSide.Left, s.CommittedSide);
            Assert.AreEqual(DecisionSide.Left, s.ToDecisionSide());
        }

        [Test]
        public void Progress_IsClampedToUnitRange()
        {
            var s = SwipeEvaluator.Evaluate(500f, 100f, 15f);
            Assert.AreEqual(1f, s.Progress, 0.0001f);
        }

        [Test]
        public void Rotation_FollowsDragDirection()
        {
            var right = SwipeEvaluator.Evaluate(50f, 100f, 20f);
            var left = SwipeEvaluator.Evaluate(-50f, 100f, 20f);
            Assert.Greater(right.Rotation, 0f);
            Assert.Less(left.Rotation, 0f);
        }
    }
}
