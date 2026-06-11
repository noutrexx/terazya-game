using System;

namespace DengeGame.Domain
{
    /// <summary>
    /// Bir olay kartının saf C# domain modeli (iskelet).
    /// Faz 5'te tüm koşul/etki/zincir alanları ile genişletilecek; ScriptableObject
    /// authoring asset'i bu modele dönüştürülecektir. Domain bu sayede Unity'siz test edilebilir kalır.
    /// </summary>
    [Serializable]
    public sealed class EventCard
    {
        public string Id;
        public string Title;
        public string CharacterId;
        public string Description;

        public string LeftText;
        public string RightText;

        public string Category;
        public int Priority;
        public float Weight = 1f;

        public int MinTurn = 0;
        public int MaxTurn = int.MaxValue;

        public bool OneShot;
        public int CooldownTurns;

        public bool IsEmergency;
        public string ChainId;

        public EventCard() { }

        public EventCard(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
