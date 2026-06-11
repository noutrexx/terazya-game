using System;

namespace DengeGame.Application
{
    /// <summary>
    /// Zaman kaynağı soyutlaması (port). UnityEngine.Time ve DateTime'ı domain/app'ten gizler;
    /// böylece zaman bağımlı mantık testlerde sahte (fake) zaman ile sınanabilir.
    /// </summary>
    public interface ITimeService
    {
        /// <summary>Saniye cinsinden kare süresi (animasyon ölçeklemesi için).</summary>
        float DeltaTime { get; }

        /// <summary>Şimdiki zaman (UTC). Kayıt zaman damgaları için.</summary>
        DateTime UtcNow { get; }
    }
}
