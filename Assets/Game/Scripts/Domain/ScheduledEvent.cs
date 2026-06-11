using System;

namespace DengeGame.Domain
{
    /// <summary>
    /// Bir kararla gelecekte tetiklenmek üzere kuyruğa alınan olay. Olay seçim servisi (Faz 5)
    /// vadesi gelen zamanlanmış olayları zorunlu olarak öne alır.
    /// </summary>
    [Serializable]
    public sealed class ScheduledEvent
    {
        public string EventId;
        public int DueTurn;

        public ScheduledEvent() { }

        public ScheduledEvent(string eventId, int dueTurn)
        {
            EventId = eventId;
            DueTurn = dueTurn;
        }
    }
}
