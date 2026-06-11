using System;

namespace DengeGame.Domain
{
    /// <summary>Bir kartın iki kararından hangisinin seçildiği.</summary>
    public enum DecisionSide
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Oyuncunun verdiği bir kararın değişmez kaydı. Karar geçmişi bu kayıtlardan oluşur.
    /// </summary>
    [Serializable]
    public sealed class DecisionRecord
    {
        public string CardId;
        public DecisionSide Side;
        public int Turn;

        public DecisionRecord() { }

        public DecisionRecord(string cardId, DecisionSide side, int turn)
        {
            CardId = cardId;
            Side = side;
            Turn = turn;
        }
    }
}
