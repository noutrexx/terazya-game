using DengeGame.Core;

namespace DengeGame.Application
{
    /// <summary>
    /// Kalıcı kayıt soyutlaması (port). Platforma özel yollar ve serileştirme implementasyonda gizlenir.
    /// Faz 10'da versiyonlama, migration ve atomik yazma ile tam olarak uygulanacaktır.
    /// </summary>
    /// <typeparam name="T">Serileştirilecek veri tipi (örn. kayıt zarfı).</typeparam>
    public interface ISaveService
    {
        bool Exists(string key);

        Result Save<T>(string key, T data);

        Result<T> Load<T>(string key);

        Result Delete(string key);
    }
}
