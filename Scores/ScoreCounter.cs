using UniRx;
using UnityEngine;
namespace Scores
{
    public class ScoreCounter : MonoBehaviour, IScoreProvider
    {
        public IReadOnlyReactiveProperty<int> Score => _score;
    
        private IntReactiveProperty _score = new IntReactiveProperty();
    
        private void Start()
        {
            MessageBroker.Default.Receive<ScoreData>().Subscribe(x =>
            {
                _score.Value += x.Value;
            }).AddTo(this);

        }

    }
}
