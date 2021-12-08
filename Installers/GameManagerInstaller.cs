using GameStates;
using Managers;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class GameManagerInstaller : MonoInstaller
    {
        [SerializeField] private GameObject gameManager;
        public override void InstallBindings()
        {
            Container
                .Bind<IGameStateProvider>()
                .To<GameManager>()
                .FromComponentOn(gameManager)
                .AsSingle()
                .NonLazy();
        }
    }
}