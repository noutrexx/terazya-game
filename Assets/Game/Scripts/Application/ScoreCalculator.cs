using DengeGame.Domain;

namespace DengeGame.Application
{
    /// <summary>Skorun bileşenlere ayrılmış dökümü (sonuç ekranında gösterilebilir).</summary>
    public sealed class ScoreBreakdown
    {
        public int TurnScore;
        public int BalanceScore;
        public int CrisisScore;
        public int ChainScore;
        public int EndingBonus;
        public int Total;
    }

    /// <summary>
    /// Yönetim skorunu hesaplar. Skor SADECE hayatta kalınan tura dayanmaz; değer dengesi,
    /// kriz çözümü, tamamlanan zincirler ve ulaşılan sonun nadirliği de katkı yapar (GDD Bölüm 6).
    ///
    /// Formül:
    ///   TurnScore    = tur * 10
    ///   BalanceScore = Σ (50 - |değer-50|)   her değer 50'ye ne kadar yakınsa o kadar puan (0..300)
    ///   CrisisScore  = max(0, çözülen*50 - başarısız*15)
    ///   ChainScore   = tamamlanan zincir * 40
    ///   EndingBonus  = nadirlik: Common 0, Uncommon 60, Rare 150
    ///   Total        = bileşenlerin toplamı (≥ 0)
    /// </summary>
    public static class ScoreCalculator
    {
        public static ScoreBreakdown Calculate(GameState state, EndingRarity rarity = EndingRarity.Common)
        {
            var b = new ScoreBreakdown();
            if (state == null) return b;

            b.TurnScore = state.CurrentTurn * 10;

            int balance = 0;
            foreach (var v in CountryValueInfo.All)
                balance += 50 - System.Math.Abs(state.Stats.Get(v) - 50);
            b.BalanceScore = balance;

            int crisis = state.ResolvedCrisisCount * 50 - state.FailedCrisisCount * 15;
            b.CrisisScore = crisis < 0 ? 0 : crisis;

            b.ChainScore = state.CompletedChainCount * 40;

            b.EndingBonus = rarity == EndingRarity.Rare ? 150 : (rarity == EndingRarity.Uncommon ? 60 : 0);

            int total = b.TurnScore + b.BalanceScore + b.CrisisScore + b.ChainScore + b.EndingBonus;
            b.Total = total < 0 ? 0 : total;
            return b;
        }
    }
}
