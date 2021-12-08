using UniRx;
namespace GameStates
{
        public interface IGameStateProvider
        {
                public ReactiveProperty<GameState> CurrentState { get; }
        }
}
