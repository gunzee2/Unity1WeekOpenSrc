using System.Linq;
using DarkTonic.MasterAudio;
using GameStates;
using Scores;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AI;
namespace Enemies
{
    public class EnemyCore : MonoBehaviour
    {
        [SerializeField] private int score;
        [SerializeField] private CapsuleCollider modelCollider;
        [SerializeField] private GameObject modelGameObject;
        [SerializeField] private GameObject ragdollPrefab;
        [SerializeField] private Transform boneParentTransform;
        [SerializeField] private NavMeshAgent navMeshAgent;

        [SerializeField] private GameObject doorHitParticle;

        private static readonly int TriggerDead = Animator.StringToHash("TriggerDead");

        private IGameStateProvider _gameStateProvider;
        private Transform _targetTransform;
        private bool _isDead = false;
        private Animator _animator;
        private Rigidbody _rb;

        /// <summary>
        /// GameStateProviderの注入
        /// </summary>
        /// <param name="provider"></param>
        public void SetGameStateProvider(IGameStateProvider provider)
        {
            _gameStateProvider = provider;
        }
        
        void Start()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();

            _targetTransform = GameObject.FindGameObjectWithTag("EnemyGoal").transform;

            // 目的地のX座標を少しだけランダムにずらす
            // (同じ場所に皆向かうと不自然に見えるため)
            this.UpdateAsObservable().Where(_ => navMeshAgent.enabled).Subscribe(_ =>
            {
                var rand = Random.Range(-0.6f, 0.6f);
                var targetVec = new Vector3(_targetTransform.position.x + rand, _targetTransform.position.y,
                    _targetTransform.position.z);
                navMeshAgent.destination = targetVec;
            }).AddTo(this);

            // ドアとの衝突処理
            // ゲーム中でかつ衝突済みでない場合、1度だけ衝突処理を実行する
            this.OnTriggerEnterAsObservable()
                .Where(x => x.CompareTag("DoorCollision"))
                .Where(_ => _gameStateProvider.CurrentState.Value == GameState.InGame)
                .Where(_ => !_isDead)
                .Take(1)
                .Subscribe(x =>
                {
                    // スコアデータ送信
                    MessageBroker.Default.Publish(new ScoreData { Value = score });

                    MasterAudio.PlaySound("hit1");
                    Instantiate(doorHitParticle, x.ClosestPoint(transform.position), Quaternion.identity);

                    StopAnimation();
                    
                    //ラグドールの生成と初期設定
                    var go = Instantiate(ragdollPrefab, modelGameObject.transform.position, modelGameObject.transform.rotation,
                        transform);
                    var children = boneParentTransform.GetComponentsInChildren<Transform>().ToArray();
                    go.GetComponent<RagdollController>().CopyBoneTransform(children);
                    go.GetComponent<RagdollController>().AddForceAllBones(-x.transform.right * 100f + (new Vector3(0, 5f, 0)));
                    // モデルを非表示にして、3秒後にGameObjectを消す
                    modelGameObject.SetActive(false);
                    Destroy(gameObject, 3f);
                    
                    _isDead = true;
                }).AddTo(this);

            // 扉を開いていない時に扉に触れた時は減点処理.
            this.OnTriggerEnterAsObservable()
                .Where(x => x.CompareTag($"EnemyGoal"))
                .Where(_ => _gameStateProvider.CurrentState.Value == GameState.InGame)
                .Where(_ => !_isDead)
                .Take(1)
                .Subscribe(x =>
                {
                    Destroy(this.gameObject);
                    _isDead = true;
                }).AddTo(this);

            // ゲームが終了したら移動とアニメーションを止める
            _gameStateProvider.CurrentState
                .Where(x => x == GameState.Result)
                .Subscribe(x =>
                {
                    navMeshAgent.enabled = false;
                    _animator.enabled = false;
                }).AddTo(this);
        }

        private void StopAnimation()
        {
            modelCollider.enabled = false;
            _animator.enabled = false;
            _rb.isKinematic = false;
            _rb.freezeRotation = false;
            navMeshAgent.enabled = false;
        }

    }
}
