using DarkTonic.MasterAudio;
using DG.Tweening;
using GameStates;
using Scores;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Obstacles
{
    public class ObstacleCore : MonoBehaviour
    {
        [SerializeField] private int score;
        [SerializeField] private Collider collider;
        [SerializeField] private GameObject explosionParticle;

        private IGameStateProvider _gameStateProvider;
        private Tween _punchTween;

        /// <summary>
        /// IGameStateProviderの注入
        /// </summary>
        /// <param name="provider"></param>
        public void SetGameStateProvider(IGameStateProvider provider)
        {
            _gameStateProvider = provider;
        }


        // Start is called before the first frame update
        void Start()
        {
            _punchTween = transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.25f, 10, 0.5f);

            // 敵のラグドールが障害物にヒットしたときの処理
            this.OnCollisionEnterAsObservable()
                .Where(x => x.collider.CompareTag($"EnemyRagdoll"))
                .Where(_ => _gameStateProvider.CurrentState.Value == GameState.InGame)
                .Take(1)
                .Subscribe(x =>
                {
                    // スコア送信
                    MessageBroker.Default.Publish(new ScoreData { Value = score });

                    // 爆発パーティクルの生成
                    Instantiate(explosionParticle, x.contacts[0].point, Quaternion.identity);

                    MasterAudio.PlaySound("obstacle_hit");

                    Destroy(gameObject);
                    Destroy(x.transform.root.gameObject);
                }).AddTo(this);

        }

        private void OnDisable()
        {
            if (DOTween.instance != null)
            {
                _punchTween?.Kill();
            }
        }
    }
}
