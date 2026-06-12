using System;

namespace DengeGame.Domain
{
    public enum EndingCategory { Disaster = 0, Neutral = 1, Success = 2, Ironic = 3 }
    public enum EndingRarity { Common = 0, Uncommon = 1, Rare = 2 }

    /// <summary>
    /// Bir yönetim sonunun veri tanımı. Sonu tetikleyen "reason" anahtarı = Id; değerlendiriciler
    /// bu anahtarı döndürür, sonuç ekranı kayıttan zengin tanımı çözer.
    /// </summary>
    [Serializable]
    public sealed class EndingDefinition
    {
        public string Id;            // tetikleyici anahtar (örn. "Devlet İflası")
        public string Title;
        public string Description;
        public EndingCategory Category;
        public EndingRarity Rarity;
        public ColorRGB ThemeColor;
        public int MetaRewardPoints;

        public EndingDefinition() { }
        public EndingDefinition(string id, string title) { Id = id; Title = title; }
    }
}
