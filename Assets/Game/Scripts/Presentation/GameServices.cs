using DengeGame.Application;
using DengeGame.Core;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Sahne kontrolcülerine açıkça enjekte edilen servis konteyneri (composition root çıktısı).
    /// Global singleton DEĞİLDİR; Bootstrap tarafından oluşturulur ve ISceneController'lara verilir.
    /// </summary>
    public sealed class GameServices
    {
        public IRandomService Random { get; }
        public ITimeService Time { get; }
        public ISaveService Save { get; }
        public IEventSelectionService EventSelection { get; }
        public ISceneTransitionService SceneTransition { get; }
        public IGameFlow Flow { get; }

        public GameServices(
            IRandomService random,
            ITimeService time,
            ISaveService save,
            IEventSelectionService eventSelection,
            ISceneTransitionService sceneTransition,
            IGameFlow flow)
        {
            Random = Guard.NotNull(random, nameof(random));
            Time = Guard.NotNull(time, nameof(time));
            Save = Guard.NotNull(save, nameof(save));
            EventSelection = Guard.NotNull(eventSelection, nameof(eventSelection));
            SceneTransition = Guard.NotNull(sceneTransition, nameof(sceneTransition));
            Flow = Guard.NotNull(flow, nameof(flow));
        }
    }

    /// <summary>
    /// Bir sahnedeki kök kontrolcü. Bootstrap, sahne yüklendiğinde servisleri buraya enjekte eder.
    /// </summary>
    public interface ISceneController
    {
        void Construct(GameServices services);
    }
}
