using System.Collections.Generic;

namespace DengeGame.Domain
{
    /// <summary>
    /// Tek bir yönetimin (oyunun) tüm runtime durumu. Saf C#; ScriptableObject verisinden
    /// tamamen ayrıdır (SO runtime'da değiştirilmez). Kayıt sistemi (Faz 10) bu modeli serileştirir.
    /// Bu iskelet Faz 4'te davranışlarla (event yayını, etki uygulama) zenginleştirilecektir.
    /// </summary>
    public sealed class GameState
    {
        public int CurrentTurn { get; private set; }
        public int CurrentYear { get; private set; }
        public GamePeriod CurrentPeriod { get; private set; }

        public CountryStats Stats { get; }

        // Aktif sistemler (id tabanlı; ayrıntılı modeller ilgili fazlarda gelir).
        public List<string> ActivePolicyIds { get; } = new List<string>();
        public List<string> ActiveCrisisIds { get; } = new List<string>();
        public Dictionary<string, int> CharacterRelationships { get; } = new Dictionary<string, int>();

        // Olay seçim algoritmasının ihtiyaç duyduğu geçmiş.
        public HashSet<string> ShownCardIds { get; } = new HashSet<string>();
        public Dictionary<string, int> LastShownTurnByCard { get; } = new Dictionary<string, int>();
        public List<DecisionRecord> DecisionHistory { get; } = new List<DecisionRecord>();
        public List<string> ActiveChainIds { get; } = new List<string>();

        public HashSet<string> Flags { get; } = new HashSet<string>();
        public HashSet<string> UnlockedAchievements { get; } = new HashSet<string>();

        public bool IsEnded { get; private set; }
        public string EndingReason { get; private set; }
        public int TotalScore { get; private set; }

        // Determinizm için seed; tüm rastgelelik bu seed üzerinden ilerletilir (GDD Bölüm 28).
        public int Seed { get; }

        public GameState(int seed, CountryStats stats = null, int turnsPerYear = 4)
        {
            Seed = seed;
            Stats = stats ?? new CountryStats();
            TurnsPerYear = turnsPerYear < 1 ? 1 : turnsPerYear;
            CurrentTurn = 0;
            CurrentYear = 1;
            CurrentPeriod = GamePeriod.Founding;
        }

        public int TurnsPerYear { get; }

        /// <summary>Turu bir ilerletir ve yıl/dönem bilgisini günceller.</summary>
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

        private static GamePeriod ResolvePeriod(int year)
        {
            if (year <= 3) return GamePeriod.Founding;
            if (year <= 8) return GamePeriod.Growth;
            if (year <= 15) return GamePeriod.Maturity;
            return GamePeriod.Legacy;
        }
    }
}
