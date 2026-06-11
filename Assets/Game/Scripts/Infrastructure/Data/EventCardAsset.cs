using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>
    /// Bir olay kartının ScriptableObject authoring varlığı. İçinde serileştirilmiş bir
    /// domain <see cref="EventCard"/> tutar. <see cref="ToDomain"/> her çağrıda DERİN KOPYA
    /// döndürür; böylece SO verisi runtime'da asla değiştirilmez (anayasa kuralı).
    /// </summary>
    [CreateAssetMenu(fileName = "Card_", menuName = "Denge/Event Card", order = 0)]
    public sealed class EventCardAsset : ScriptableObject
    {
        [SerializeField] private EventCard card = new EventCard();

        /// <summary>Authoring erişimi (yalnızca Editor/araçlar için). Runtime'da ToDomain kullanın.</summary>
        public EventCard Source => card;

        /// <summary>Runtime kullanımı için bağımsız (derin kopyalanmış) domain modeli döndürür.</summary>
        public EventCard ToDomain() => JsonUtility.FromJson<EventCard>(JsonUtility.ToJson(card));
    }
}
