using System.Threading;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using Doozy.Engine.UI;
using GameStates;
using Scores;
using Times;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
namespace Managers
{
    public class GameManager : MonoBehaviour, IGameStateProvider
    {
        public ReactiveProperty<GameState> CurrentState { get; set; }
        
        [SerializeField] private TMP_Text readyText;
        [SerializeField] private Button startButton;
        [SerializeField] private Transform titlePanelTransform;
        
        [Inject] private IScoreProvider _scoreProvider;
        [Inject] private ITimerProvider _timeProvider;

        private void Awake()
        {
            CurrentState = new ReactiveProperty<GameState>(GameState.Initialize);

            CurrentState.Subscribe(x =>
            {
                switch (x)
                {
                    case GameState.Initialize:
                        InitializeGameSequence(this.GetCancellationTokenOnDestroy()).Forget();
                        break;
                    case GameState.InGame:
                        InGameSequence(this.GetCancellationTokenOnDestroy()).Forget();
                        break;
                    case GameState.Result:
                        ResultSequence(this.GetCancellationTokenOnDestroy()).Forget();
                        break;
                }
            }).AddTo(this);

        }

        async UniTaskVoid InitializeGameSequence(CancellationToken token)
        {
            await titlePanelTransform.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
            await startButton.OnClickAsync(token);
            await titlePanelTransform.GetComponent<CanvasGroup>().DOFade(0f, 0.25f);
            titlePanelTransform.GetComponent<CanvasGroup>().interactable = false;

            await UniTask.Delay(500, false, PlayerLoopTiming.Update, token);

            await readyText.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
            await UniTask.Delay(750, false, PlayerLoopTiming.Update, token);
            MasterAudio.PlaySound("gunshot");
            readyText.text = "GO!";
            await readyText.transform.DOPunchScale(new Vector3(2, 2, 2), 0.25f);
            readyText.GetComponent<CanvasGroup>().DOFade(0f, 0.25f);

            CurrentState.Value = GameState.InGame;
        }

        async UniTaskVoid InGameSequence(CancellationToken token)
        {
            _timeProvider.GameTimerCountDownStart();
            MasterAudio.PlaySound("bgm");
            await UniTask.WaitUntil(() => _timeProvider.CountDownTimer.Value <= 0, PlayerLoopTiming.Update, token);
            MasterAudio.FadeOutAllOfSound("bgm", 0.5f);
            MasterAudio.PlaySound("whistle");
            CurrentState.Value = GameState.Result;
        }

        async UniTaskVoid ResultSequence(CancellationToken token)
        {
            readyText.text = "Finish!!";
            readyText.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
            await readyText.transform.DOPunchScale(new Vector3(2, 2, 2), 0.25f);
            await UniTask.Delay(750, false, PlayerLoopTiming.Update, token);
            await readyText.GetComponent<CanvasGroup>().DOFade(0f, 0.25f);

            var popup = UIPopupManager.GetPopup("ResultPopup");
            popup.Data.SetLabelsTexts($"SCORE:{_scoreProvider.Score.Value}");
            popup.Data.SetButtonsCallbacks(RetryButtonClicked);
            UIPopupManager.ShowPopup(popup, popup.AddToPopupQueue, false);
        }

        private void RetryButtonClicked()
        {
            DOTween.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
