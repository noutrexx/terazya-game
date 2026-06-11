using System;
using DengeGame.Application;
using UnityEngine;

namespace DengeGame.Infrastructure
{
    /// <summary>
    /// ITimeService'in Unity implementasyonu. Animasyon azaltma ayarına saygı duyacak şekilde
    /// Faz 13'te genişletilebilir.
    /// </summary>
    public sealed class UnityTimeService : ITimeService
    {
        public float DeltaTime => Time.deltaTime;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
