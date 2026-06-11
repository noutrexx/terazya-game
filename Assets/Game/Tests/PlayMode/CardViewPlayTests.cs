using System.Collections;
using DengeGame.Domain;
using DengeGame.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DengeGame.Tests.PlayMode
{
    public sealed class CardViewPlayTests
    {
        private GameObject _canvasGo;

        [SetUp]
        public void SetUp()
        {
            UiPreferences.ReduceMotion = true; // varsayılan: anlık (deterministik)
            _canvasGo = new GameObject("Canvas", typeof(Canvas));
        }

        [TearDown]
        public void TearDown()
        {
            UiPreferences.ReduceMotion = false;
            if (_canvasGo != null) Object.DestroyImmediate(_canvasGo);
        }

        private CardView NewCard()
        {
            var card = CardView.Create(_canvasGo.transform);
            var c = new EventCard("x", "Başlık")
            {
                CharacterId = "maliye",
                Description = "Bir test kararı.",
                LeftText = "Sol",
                RightText = "Sağ"
            };
            c.LeftEffects.Add(EffectData.ChangeValue(CountryValue.Economy, -3));
            c.RightEffects.Add(EffectData.ChangeValue(CountryValue.Economy, 3));
            card.Setup(c);
            return card;
        }

        [Test]
        public void ForceCommit_FiresCommittedOnce()
        {
            var card = NewCard();
            int count = 0;
            DecisionSide captured = DecisionSide.Left;
            card.Committed += s => { count++; captured = s; };

            card.ForceCommit(DecisionSide.Right);

            Assert.AreEqual(1, count);
            Assert.AreEqual(DecisionSide.Right, captured);
        }

        [Test]
        public void SecondCommit_IsIgnored_NoDoubleApply()
        {
            var card = NewCard();
            int count = 0;
            card.Committed += _ => count++;

            card.ForceCommit(DecisionSide.Left);
            card.ForceCommit(DecisionSide.Right); // yok sayılmalı

            Assert.AreEqual(1, count);
            Assert.IsFalse(card.Interactable);
        }

        [Test]
        public void StatBarView_SetValue_UpdatesWithoutErrors()
        {
            var bar = StatBarView.Create(_canvasGo.transform, CountryValue.Security, "Güv");
            bar.SetValue(10, animate: false);  // kritik
            bar.SetValue(75, animate: false);
            Assert.AreEqual(CountryValue.Security, bar.Value);
        }

        [UnityTest]
        public IEnumerator AnimatedCommit_BlocksInput_AndFiresOnce()
        {
            UiPreferences.ReduceMotion = false; // animasyonlu yol
            var card = NewCard();
            int count = 0;
            card.Committed += _ => count++;

            card.ForceCommit(DecisionSide.Right);
            Assert.IsFalse(card.Interactable); // animasyon sırasında input kapalı

            // İkinci taahhüt animasyon sırasında yok sayılmalı.
            card.ForceCommit(DecisionSide.Left);

            int frames = 0;
            while (count == 0 && frames < 240) { frames++; yield return null; }

            Assert.AreEqual(1, count);
        }
    }
}
