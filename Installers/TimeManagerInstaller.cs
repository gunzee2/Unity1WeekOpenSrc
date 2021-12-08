using Managers;
using Times;
using UnityEngine;
using Zenject;
namespace Installers
{
    public class TimeManagerInstaller : MonoInstaller
    {
        [SerializeField] private GameObject timeManager;
        public override void InstallBindings()
        {
            Container
                .Bind<ITimerProvider>()
                .To<TimeController>()
                .FromComponentOn(timeManager)
                .AsSingle()
                .NonLazy();
        }
    }
}