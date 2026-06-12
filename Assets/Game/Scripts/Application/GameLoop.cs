using System;
using System.Collections.Generic;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Application.Policies;
using DengeGame.Core;
using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>
    /// Çekirdek oyun döngüsü orkestratörü (saf C#, Unity'siz → test edilebilir). UI bu sınıfı sürer:
    /// kart seç → sun → karar uygula → tur ilerlet → son denetle → tekrar veya bitir.
    /// Bir karar iki kez uygulanamaz (her Decide yeni kartı sunar veya oyunu bitirir).
    /// </summary>
    public sealed class GameLoop
    {
        private readonly GameSession _session;
        private readonly IReadOnlyList<EventCard> _pool;
        private readonly IEventSelectionService _selection;
        private readonly IDecisionEffectService _effects;
        private readonly IRandomService _random;
        private readonly IEndingEvaluator _endings;
        private readonly PolicyCrisisProcessor _processor;

        public EventCard CurrentCard { get; private set; }

        public bool IsRunning => !_session.State.IsEnded && CurrentCard != null;

        /// <summary>Yeni bir kart sunulduğunda yayınlanır.</summary>
        public event Action<EventCard> CardPresented;

        /// <summary>Yönetim sona erdiğinde yayınlanır (sebep).</summary>
        public event Action<string> Ended;

        public GameLoop(GameSession session, IReadOnlyList<EventCard> pool,
            IEventSelectionService selection, IDecisionEffectService effects,
            IRandomService random, IEndingEvaluator endings,
            PolicyCrisisProcessor processor = null)
        {
            _session = Guard.NotNull(session, nameof(session));
            _pool = Guard.NotNull(pool, nameof(pool));
            _selection = Guard.NotNull(selection, nameof(selection));
            _effects = Guard.NotNull(effects, nameof(effects));
            _random = Guard.NotNull(random, nameof(random));
            _endings = Guard.NotNull(endings, nameof(endings));
            _processor = processor; // opsiyonel: politika/kriz yaşam döngüsü
        }

        public GameSession Session => _session;

        /// <summary>İlk kartı seçip sunar.</summary>
        public Result Begin()
        {
            if (_session.State.IsEnded) return Result.Failure("Yönetim zaten sona erdi.");
            return SelectAndPresent();
        }

        /// <summary>Mevcut karta verilen kararı uygular ve döngüyü ilerletir.</summary>
        public Result Decide(DecisionSide side)
        {
            if (!IsRunning) return Result.Failure("Uygulanacak aktif kart yok.");

            var card = CurrentCard;
            CurrentCard = null; // aynı karar iki kez uygulanamaz

            var effectList = EffectFactory.CreateMany(card.EffectsFor(side));
            var applied = _effects.ApplyEffects(_session.State, effectList, _random, card.Id);
            if (applied.IsFailure)
            {
                CurrentCard = card; // başarısızsa kartı geri ver
                return applied;
            }

            _session.State.RecordDecision(new DecisionRecord(card.Id, side, _session.State.CurrentTurn));

            // Karar oyunu bitirmediyse turu ilerlet (süreli etkiler işlenir).
            if (!_session.State.IsEnded)
                _session.Turns.AdvanceTurn();

            // Politika/kriz yaşam döngüsü (her tur).
            if (!_session.State.IsEnded)
                _processor?.Process(_session.State, _effects, _random);

            // Son denetimi (önce özel EndGameEffect, sonra sınır sonları).
            if (!_session.State.IsEnded)
            {
                var reason = _endings.Evaluate(_session.State);
                if (!string.IsNullOrEmpty(reason))
                    _session.Turns.EndGame(reason);
            }

            if (_session.State.IsEnded)
            {
                Ended?.Invoke(_session.State.EndingReason);
                return Result.Success();
            }

            return SelectAndPresent();
        }

        private Result SelectAndPresent()
        {
            var selected = _selection.SelectNext(_session.State, _pool, _random);
            if (selected.IsFailure)
            {
                _session.Turns.EndGame("Gösterilecek olay kalmadı.");
                Ended?.Invoke(_session.State.EndingReason);
                return Result.Failure(selected.Error);
            }

            CurrentCard = selected.Value;
            _session.State.MarkCardShown(CurrentCard.Id); // cooldown/tek-kullanımlık için
            CardPresented?.Invoke(CurrentCard);
            return Result.Success();
        }
    }
}
