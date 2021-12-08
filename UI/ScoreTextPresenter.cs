using Scores;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
namespace UI
{
    public class ScoreTextPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text textView;
        
        [Inject] private IScoreProvider _scoreProvider;
        
        private void Start()
        {
            _scoreProvider.Score.Subscribe(x =>
            {
                textView.text = $"SCORE:{x}";
            }).AddTo(this);
        }

    }
}
