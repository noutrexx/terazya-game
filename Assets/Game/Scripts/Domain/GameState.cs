using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>
    /// Tek bir yönetimin (oyunun) tüm runtime durumu. Saf C#; ScriptableObject verisinden
    /// tamamen ayrıdır (SO runtime'da değiştirilmez). Kayıt sistemi (Faz 10) bu modeli serileştirir.
    /// </summary>
    public sealed class GameState
    {
        public const int RelationshipMin = -100;
        public const int RelationshipMax = 100;

        public int CurrentTurn { get; private set; }
        public int CurrentYear { get; private set; }
        public GamePeriod CurrentPeriod { get; private set; }

        public CountryStats Stats { get; }

        public List<string> ActivePolicyIds { get; } = new List<string>();
        public List<string> ActiveCrisisIds { get; } = new List<string>();
        public Dictionary<string, int> CharacterRelationships { get; } = new Dictionary<string, int>();

        public HashSet<string> ShownCardIds { get; } = new HashSet<string>();
        public Dictionary<string, int> LastShownTurnByCard { get; } = new Dictionary<string, int>();
        public List<DecisionRecord> DecisionHistory { get; } = new List<DecisionRecord>();
        public List<string> ActiveChainIds { get; } = new List<string>();

        public List<ScheduledEvent> ScheduledEvents { get; } = new List<ScheduledEvent>();
        public List<ActiveTimedEffect> ActiveTimedEffects { get; } = new List<ActiveTimedEffect>();

        public HashSet<string> Flags { get; } = new HashSet<string>();
        public HashSet<string> UnlockedAchievements { get; } = new HashSet<string>();

        public bool IsEnded { get; private set; }
        public string EndingReason { get; private set; }
        public int TotalScore { get; private set; }

        public int Seed { get; }
        public int TurnsPerYear { get; }

        public GameState(int seed, CountryStats stats = null, int turnsPerYear = 4)
        {
            Seed = seed;
            Stats = stats ?? new CountryStats();
            TurnsPerYear = turnsPerYear < 1 ? 1 : turnsPerYear;
            CurrentTurn = 0;
            CurrentYear = 1;
            CurrentPeriod = GamePeriod.Founding;
        }

        // --- Tur akışı ---

        public void AdvanceTurn()
        {
            CurrentTurn++;
            CurrentYear = 1 + CurrentTurn / TurnsPerYear;
            CurrentPeriod = ResolvePeriod(CurrentYear);
        }

        public void MarkCardShown(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return;
            ShownCardIds.Add(cardId);
            LastShownTurnByCard[cardId] = CurrentTurn;
        }

        public void RecordDecision(DecisionRecord record)
        {
            if (record != null) DecisionHistory.Add(record);
        }

        public void EndGame(string reason)
        {
            IsEnded = true;
            EndingReason = reason;
        }

        public void SetScore(int score) => TotalScore = score;

        // --- Bayraklar ---

        public void SetFlag(string flag, bool enabled)
        {
            if (string.IsNullOrEmpty(flag)) return;
            if (enabled) Flags.Add(flag);
            else Flags.Remove(flag);
        }

        public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && Flags.Contains(flag);

        // --- İlişkiler (-100..100) ---

        public int GetRelationship(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return 0;
            return CharacterRelationships.TryGetValue(characterId, out int v) ? v : 0;
        }

        /// <summary>İlişkiye delta uygular (clamp). Gerçekleşen net değişimi döndürür.</summary>
        public int ApplyRelationship(string characterId, int delta)
        {
            if (string.IsNullOrEmpty(characterId)) return 0;
            int previous = GetRelationship(characterId);
            int current = ClampRelationship(previous + delta);
            CharacterRelationships[characterId] = current;
            return current - previous;
        }

        // --- Politikalar / krizler ---

        public bool StartPolicy(string policyId)
        {
            if (string.IsNullOrEmpty(policyId) || ActivePolicyIds.Contains(policyId)) return false;
            ActivePolicyIds.Add(policyId);
            return true;
        }

        public bool EndPolicy(string policyId) => ActivePolicyIds.Remove(policyId);

        public bool StartCrisis(string crisisId)
        {
            if (string.IsNullOrEmpty(crisisId) || ActiveCrisisIds.Contains(crisisId)) return false;
            ActiveCrisisIds.Add(crisisId);
            return true;
        }

        public bool EndCrisis(string crisisId) => ActiveCrisisIds.Remove(crisisId);

        // --- Zamanlama / süreli etkiler ---

        public void ScheduleEvent(string eventId, int dueTurn)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            ScheduledEvents.Add(new ScheduledEvent(eventId, dueTurn));
        }

        public void AddTimedEffect(ActiveTimedEffect effect)
        {
            if (effect != null && effect.RemainingTurns > 0)
                ActiveTimedEffects.Add(effect);
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!string.IsNullOrEmpty(achievementId))
                UnlockedAchievements.Add(achievementId);
        }

        private static int ClampRelationship(int v)
        {
            if (v < RelationshipMin) return RelationshipMin;
            if (v > RelationshipMax) return RelationshipMax;
            return v;
        }

        private static GamePeriod ResolvePeriod(int year)
        {
            if (year <= 3) return GamePeriod.Founding;
            if (year <= 8) return GamePeriod.Growth;
            if (year <= 15) return GamePeriod.Maturity;
            return GamePeriod.Legacy;
        }
    }
}
