using System;

namespace DengeGame.Domain
{
    /// <summary>
    /// Belirli sayıda tur boyunca her tur bir değere delta uygulayan aktif etki.
    /// TurnManager her tur ilerleyişinde bu etkileri işler ve süresi dolanları kaldırır.
    /// </summary>
    [Serializable]
    public sealed class ActiveTimedEffect
    {
        public CountryValue Value;
        public int PerTurnDelta;
        public int RemainingTurns;
        public string Reason;

        public ActiveTimedEffect() { }

        public ActiveTimedEffect(CountryValue value, int perTurnDelta, int remainingTurns, string reason)
        {
            Value = value;
            PerTurnDelta = perTurnDelta;
            RemainingTurns = remainingTurns;
            Reason = reason;
        }
    }
}
