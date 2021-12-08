using UniRx;
namespace Scores
{
    public interface IScoreProvider
    {
        public IReadOnlyReactiveProperty<int> Score { get; }
    }
}
