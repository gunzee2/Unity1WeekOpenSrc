using Scores;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ScoreCounterInstaller : MonoInstaller
    {
        [SerializeField] private GameObject scoreCounter;
        public override void InstallBindings()
        {
            Container
                .Bind<IScoreProvider>()
                .To<ScoreCounter>()
                .FromComponentOn(scoreCounter)
                .AsSingle()
                .NonLazy();
        }
    }
}
