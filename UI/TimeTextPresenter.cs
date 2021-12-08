using Times;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
namespace UI
{
    public class TimeTextPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text textView;
        
        [Inject] private ITimerProvider _timerProvider;

        private void Start()
        {
            _timerProvider.CountDownTimer.Subscribe(x =>
            {
                textView.text = $"Time:{x:00}";
            }).AddTo(this);
        }

    }
}
