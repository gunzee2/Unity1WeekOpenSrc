using UniRx;
namespace Times
{
    public interface ITimerProvider
    {
        public IReadOnlyReactiveProperty<int> CountDownTimer { get; }

        public void GameTimerCountDownStart();
    }
}
