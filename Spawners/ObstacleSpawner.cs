using System;
using System.Collections.Generic;
using System.Linq;
using DarkTonic.MasterAudio;
using GameStates;
using Obstacles;
using Times;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Spawners
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objectPrefabs;
        
        [Inject] private IGameStateProvider _gameStateProvider;
        [Inject] private ITimerProvider _timerProvider;
        
        private const float PLANE_SIZE = 10;
        private const int SPAWN_RETRY_COUNT = 20;

        private string _objectTag;
        /// <summary>
        /// 障害物を生成する範囲
        /// </summary>
        private Rect _planeRect;
        private IDisposable _disposable;
        
        private void Awake()
        {
            _planeRect = new Rect(
                transform.position.x - (PLANE_SIZE * transform.localScale.x) / 2,
                transform.position.z - (PLANE_SIZE * transform.localScale.z) / 2,
                PLANE_SIZE * transform.localScale.x, PLANE_SIZE * transform.localScale.z);

            _objectTag = objectPrefabs[0].tag;
        }

        // Start is called before the first frame update
        private void Start()
        {
            var timeSplit = _timerProvider.CountDownTimer.Value / 3;
            _gameStateProvider.CurrentState.Subscribe(x =>
            {
                switch (x)
                {
                    case GameState.InGame:
                        // 経過時間に合わせて生成物の内容や頻度を変える
                        _timerProvider.CountDownTimer.Where(y => y <= timeSplit * 3).Take(1).Subscribe(_ =>
                        {
                            _disposable?.Dispose();

                            var rand = Random.Range(2f, 3f);
                            _disposable = Observable.Interval(TimeSpan.FromSeconds(rand)).Subscribe(_ =>
                            {
                                SpawnRandomObject();
                            }).AddTo(this);

                        }).AddTo(this);
                        _timerProvider.CountDownTimer.Where(y => y <= timeSplit * 2).Take(1).Subscribe(_ =>
                        {
                            _disposable?.Dispose();

                            var rand = Random.Range(1.5f, 2f);
                            _disposable = Observable.Interval(TimeSpan.FromSeconds(rand)).Subscribe(_ =>
                            {
                                SpawnRandomObject();
                            }).AddTo(this);

                        }).AddTo(this);
                        _timerProvider.CountDownTimer.Where(y => y <= timeSplit).Take(1).Subscribe(_ =>
                        {
                            _disposable?.Dispose();

                            var rand = Random.Range(0.5f, 1f);
                            _disposable = Observable.Interval(TimeSpan.FromSeconds(rand)).Subscribe(_ =>
                            {
                                SpawnRandomObject();
                            }).AddTo(this);

                        }).AddTo(this);
                        break;
                    case GameState.Result:
                        _disposable.Dispose();
                        break;
                }
            }).AddTo(this);
        }

        private void SpawnRandomObject()
        {
            var objectPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
            Spawn(objectPrefab);
        }

        private void Spawn(GameObject prefab)
        {
            for (var i = 0; i < SPAWN_RETRY_COUNT; i++)
            {
                // Planeの範囲内でランダムな座標を指定し、障害物の生成候補座標とする
                var randX = Random.Range(_planeRect.xMin, _planeRect.xMax);
                var randZ = Random.Range(_planeRect.yMin, _planeRect.yMax);

                var randomPos = new Vector3(randX, transform.position.y, randZ);

                // 生成したい場所に既に障害物が存在しているかチェック、存在する場合は座標を決め直す
                if (Physics.OverlapSphere(randomPos, 1).Any(col => col.CompareTag(_objectTag))) continue;

                MasterAudio.PlaySound("pom");
                var go = Instantiate(prefab, randomPos, Quaternion.identity);
            
                // GameStateProviderを注入
                go.GetComponent<ObstacleCore>().SetGameStateProvider(_gameStateProvider);
            
                // Y軸をランダムに回転(自然に見えるように)
                go.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));

                // 生成に成功したのでループを抜ける
                break;
            }

        }




    }
}
