using DengeGame.Domain;

namespace DengeGame.Presentation.UI
{
    /// <summary>Ülke değerlerinin TR görünen adları (i18n-hazır: tek yerde tutulur).</summary>
    public static class CountryValueLabels
    {
        public static string Short(CountryValue v)
        {
            switch (v)
            {
                case CountryValue.Economy: return "Eko";
                case CountryValue.PublicSupport: return "Halk";
                case CountryValue.Security: return "Güv";
                case CountryValue.Freedom: return "Özg";
                case CountryValue.Environment: return "Çev";
                case CountryValue.Diplomacy: return "Dip";
                default: return v.ToString();
            }
        }

        public static string Full(CountryValue v)
        {
            switch (v)
            {
                case CountryValue.Economy: return "Ekonomi";
                case CountryValue.PublicSupport: return "Halk Desteği";
                case CountryValue.Security: return "Güvenlik";
                case CountryValue.Freedom: return "Özgürlük";
                case CountryValue.Environment: return "Çevre";
                case CountryValue.Diplomacy: return "Diplomasi";
                default: return v.ToString();
            }
        }
    }
}
