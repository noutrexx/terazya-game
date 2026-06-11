using System;

namespace DengeGame.Domain
{
    /// <summary>Unity'siz, serileştirilebilir basit renk (Presentation katmanında Color'a çevrilir).</summary>
    [Serializable]
    public struct ColorRGB
    {
        public float R, G, B;
        public ColorRGB(float r, float g, float b) { R = r; G = g; B = b; }
    }

    /// <summary>
    /// Bir karakterin/kurumun saf C# domain modeli. Görsel asset kullanılmaz; UI temsili
    /// baş harf avatarı + tema rengi + rol ile oluşturulur. ScriptableObject authoring'den çevrilir.
    /// </summary>
    [Serializable]
    public sealed class Character
    {
        public string Id;
        public string Name;
        public string Role;
        public string Description;
        public ColorRGB ThemeColor;

        /// <summary>Düşükten yükseğe 5 ilişki kademesi adı (örn. Düşman→Sadık). Boşsa varsayılan kullanılır.</summary>
        public string[] RelationshipTierNames;

        /// <summary>Düşük ve yüksek ilişki anlatımları (detay ekranında gösterilir).</summary>
        public string LowOutcome;
        public string HighOutcome;

        // Opsiyonel özel sonlar: ilişki eşik aşılırsa yönetim sonu tetiklenir.
        public bool EnableLowEnding;
        public int LowEndingThreshold = -80;
        public string LowEndingReason;

        public bool EnableHighEnding;
        public int HighEndingThreshold = 80;
        public string HighEndingReason;

        public Character() { }
        public Character(string id, string name) { Id = id; Name = name; }
    }

    /// <summary>Bir ilişki değişiminin değişmez kaydı (ilişki geçmişi).</summary>
    [Serializable]
    public sealed class RelationshipChange
    {
        public string CharacterId;
        public int Delta;
        public int ResultingValue;
        public int Turn;

        public RelationshipChange() { }
        public RelationshipChange(string characterId, int delta, int resultingValue, int turn)
        {
            CharacterId = characterId; Delta = delta; ResultingValue = resultingValue; Turn = turn;
        }
    }
}
