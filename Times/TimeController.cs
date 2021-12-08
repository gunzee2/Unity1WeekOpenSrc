using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
namespace Times
{
    public class TimeController : MonoBehaviour, ITimerProvider
    {
        public IReadOnlyReactiveProperty<int> CountDownTimer => countDownTimer;
        
        [SerializeField] private IntReactiveProperty countDownTimer = new IntReactiveProperty(10);


        public void GameTimerCountDownStart()
        {
            CountSequence(countDownTimer, this.GetCancellationTokenOnDestroy()).Forget();
        }

        async UniTaskVoid CountSequence(IntReactiveProperty timer, CancellationToken cancellationToken)
        {
            while (timer.Value > 0)
            {
                timer.SetValueAndForceNotify(timer.Value - 1);
                await UniTask.Delay(1000, DelayType.Realtime, PlayerLoopTiming.Update, cancellationToken);
            }
        }

    }
}
