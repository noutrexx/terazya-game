using System;
using System.Collections.Generic;
using DengeGame.Infrastructure;
using DengeGame.Infrastructure.Data;
using DengeGame.Application;
using DengeGame.Application.Effects;
using DengeGame.Application.Endings;
using DengeGame.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Composition root. Tüm servisleri tek yerde oluşturur (manuel DI; framework yok) ve
    /// sahne yüklendikçe ISceneController'lara açıkça enjekte eder. DontDestroyOnLoad ile yaşar.
    /// Domain/Application bu sınıfa bağımlı DEĞİLDİR; bağımlılık yalnızca Presentation → çekirdek yönündedir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Bootstrap : MonoBehaviour
    {
        private static Bootstrap _instance;

        private GameServices _services;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildServices();

            SceneManager.sceneLoaded += OnSceneLoaded;
            InjectControllers(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _instance = null;
            }
        }

        private void BuildServices()
        {
            int bootSeed = Environment.TickCount;

            IRandomService random = new SystemRandomService(bootSeed);
            ITimeService time = new UnityTimeService();
            ISaveService save = new JsonFileSaveService();
            IEventSelectionService selection = new WeightedEventSelectionService();
            IDecisionEffectService decisions = new DecisionEffectService();
            ISceneTransitionService transition = new SceneTransitionService();
            IEndingEvaluator endings = new BoundaryEndingEvaluator();

            var database = Resources.Load<EventCardDatabase>(EventCardDatabase.ResourcesPath);
            if (database == null)
                Debug.LogError($"[Denge] EventCardDatabase Resources'ta bulunamadı " +
                               $"('{EventCardDatabase.ResourcesPath}'). 'Denge/İçerik/Test Kartları Üret' çalıştırın.");
            Func<List<EventCard>> poolProvider = () =>
                database != null ? database.BuildPool() : new List<EventCard>();

            IGameFlow flow = new GameFlow(transition, () => Environment.TickCount,
                poolProvider, selection, decisions, random, endings);

            _services = new GameServices(random, time, save, selection, decisions, transition, flow);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InjectControllers(scene);

        private void InjectControllers(Scene scene)
        {
            if (!scene.IsValid() || _services == null) return;

            foreach (var root in scene.GetRootGameObjects())
            {
                var controllers = root.GetComponentsInChildren<ISceneController>(true);
                foreach (var controller in controllers)
                    controller.Construct(_services);
            }
        }
    }

    /// <summary>
    /// Bootstrap'ı ilk sahne yüklenmeden önce otomatik oluşturur; böylece her sahneye elle
    /// Bootstrap eklemek gerekmez ve oyun herhangi bir sahneden Play edilse de çalışır.
    /// </summary>
    public static class GameEntryPoint
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var go = new GameObject("[Denge Bootstrap]");
            go.AddComponent<Bootstrap>();
        }
    }
}
