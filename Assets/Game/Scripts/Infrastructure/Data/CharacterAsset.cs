using DengeGame.Domain;
using UnityEngine;

namespace DengeGame.Infrastructure.Data
{
    /// <summary>
    /// Bir karakterin/kurumun ScriptableObject authoring varlığı. ToDomain her çağrıda derin
    /// kopya döndürür; SO verisi runtime'da değiştirilmez (anayasa kuralı).
    /// </summary>
    [CreateAssetMenu(fileName = "Character_", menuName = "Denge/Character", order = 2)]
    public sealed class CharacterAsset : ScriptableObject
    {
        [SerializeField] private Character character = new Character();

        public Character Source => character;

        public Character ToDomain() => JsonUtility.FromJson<Character>(JsonUtility.ToJson(character));
    }
}
