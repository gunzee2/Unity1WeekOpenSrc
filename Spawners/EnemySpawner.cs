using System;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using GameStates;
using Times;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Spawners
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objectPrefabs;
        
        [Inject] private IGameStateProvider _gameStateProvider;
        [Inject] private ITimerProvider _timerProvider;
    
        private const float PLANE_SIZE = 10;
        private const int SPAWN_RETRY_COUNT = 20;

        private string _objectTag;
        /// <summary>
        /// 敵を生成する範囲
        /// </summary>
        private Rect _planeInfo;
        private IDisposable _disposable;
        
        private void Awake()
        {
            _planeInfo = new Rect(
                transform.position.x - (PLANE_SIZE * transform.localScale.x) / 2, 
                transform.position.z - (PLANE_SIZE * transform.localScale.z) / 2,
                PLANE_SIZE * transform.localScale.x, PLANE_SIZE * transform.localScale.z);

            _objectTag = objectPrefabs[0].tag;
        }

        private void Start()
        {
            var timeSplit = _timerProvider.CountDownTimer.Value / 3;
            _gameStateProvider.CurrentState.Subscribe(x =>
            {
                switch (x)
                {
                    case GameState.InGame:
                        _timerProvider.CountDownTimer.Where(y => y <= timeSplit * 3).Take(1).Subscribe(_ =>
                        {
                            _disposable?.Dispose();

                            var rand = Random.Range(1f, 1.5f);
                            _disposable = Observable.Interval(TimeSpan.FromSeconds(rand)).Subscribe(_ =>
                            {
                                SpawnRandomObject();
                            }).AddTo(this);
                        
                        }).AddTo(this);
                        _timerProvider.CountDownTimer.Where(y => y <= timeSplit * 2).Take(1).Subscribe(_ =>
                        {
                            _disposable?.Dispose();

                            var rand = Random.Range(0.8f, 1f);
                            _disposable = Observable.Interval(TimeSpan.FromSeconds(rand)).Subscribe(_ =>
                            {
                                SpawnRandomObject();
                            }).AddTo(this);
                        
                        }).AddTo(this);
                        _timerProvider.CountDownTimer.Where(y => y <= timeSplit).Take(1).Subscribe(_ =>
                        {
                            _disposable?.Dispose();

                            var rand = Random.Range(0.6f, 0.8f);
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
            var objectPrefab = objectPrefabs.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            Spawn(objectPrefab);
        }

        private void Spawn(GameObject prefab)
        {
            for (var i = 0; i < SPAWN_RETRY_COUNT; i++)
            {
                var randX = Random.Range(_planeInfo.xMin, _planeInfo.xMax);
                var randZ = Random.Range(_planeInfo.yMin, _planeInfo.yMax);

                var randomPos = new Vector3(randX, transform.position.y, randZ);

                if (Physics.OverlapSphere(randomPos, 1).Any(col => col.CompareTag(_objectTag))) continue;
                
                var go = Instantiate(prefab, randomPos, Quaternion.identity);
                go.GetComponent<EnemyCore>().SetGameStateProvider(_gameStateProvider);
                    
                break;
            }
        
        }

    
    

    }
}
