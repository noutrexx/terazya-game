using DengeGame.Domain;

namespace DengeGame.Application.Endings
{
    /// <summary>
    /// Herhangi bir değer 0 veya 100 sınırına ulaştığında ilgili yönetim sonunu döndürür
    /// (GDD Bölüm 8). Değerler sabit sırada denetlenir → deterministik.
    /// </summary>
    public sealed class BoundaryEndingEvaluator : IEndingEvaluator
    {
        public string Evaluate(GameState state)
        {
            if (state == null || state.IsEnded) return null;

            foreach (var v in CountryValueInfo.All)
            {
                int value = state.Stats.Get(v);
                if (value <= CountryValueInfo.Min) return LowEnding(v);
                if (value >= CountryValueInfo.Max) return HighEnding(v);
            }
            return null;
        }

        private static string LowEnding(CountryValue v)
        {
            switch (v)
            {
                case CountryValue.Economy: return "Devlet İflası";
                case CountryValue.PublicSupport: return "Halk Ayaklanması";
                case CountryValue.Security: return "Kontrolün Kaybı";
                case CountryValue.Freedom: return "Otoriter Rejim";
                case CountryValue.Environment: return "Ekolojik Çöküş";
                case CountryValue.Diplomacy: return "Uluslararası İzolasyon";
                default: return "Yönetim Sona Erdi";
            }
        }

        private static string HighEnding(CountryValue v)
        {
            switch (v)
            {
                case CountryValue.Economy: return "Şirketokrasinin Yükselişi";
                case CountryValue.PublicSupport: return "Popülizm Çöküşü";
                case CountryValue.Security: return "Baskıcı Güvenlik Rejimi";
                case CountryValue.Freedom: return "Otorite Boşluğu";
                case CountryValue.Environment: return "Yeşil Durgunluk";
                case CountryValue.Diplomacy: return "Egemenlik Kaybı";
                default: return "Yönetim Sona Erdi";
            }
        }
    }
}
