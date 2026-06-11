using DengeGame.Core;
using NUnit.Framework;

namespace DengeGame.Tests.EditMode
{
    public sealed class ResultTests
    {
        [Test]
        public void Success_HasNoError()
        {
            var r = Result.Success();
            Assert.IsTrue(r.IsSuccess);
            Assert.IsFalse(r.IsFailure);
            Assert.IsNull(r.Error);
        }

        [Test]
        public void Failure_CarriesError()
        {
            var r = Result.Failure("hata");
            Assert.IsTrue(r.IsFailure);
            Assert.AreEqual("hata", r.Error);
        }

        [Test]
        public void GenericSuccess_CarriesValue()
        {
            var r = Result.Success(42);
            Assert.IsTrue(r.IsSuccess);
            Assert.AreEqual(42, r.Value);
        }

        [Test]
        public void GenericFailure_ReturnsFallback()
        {
            var r = Result.Failure<int>("yok");
            Assert.IsTrue(r.IsFailure);
            Assert.AreEqual(-1, r.GetValueOrDefault(-1));
        }
    }
}
