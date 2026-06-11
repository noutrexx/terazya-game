using DengeGame.Domain;

namespace DengeGame.Application.Endings
{
    /// <summary>
    /// Mevcut duruma göre yönetimin sona erip ermediğini değerlendirir.
    /// Sona eriyorsa son sebebini (başlık) döndürür; aksi halde null.
    /// Faz 9 bunu tam son sistemiyle (karakter/kriz/zincir/süre sonları) genişletir.
    /// </summary>
    public interface IEndingEvaluator
    {
        string Evaluate(GameState state);
    }
}
