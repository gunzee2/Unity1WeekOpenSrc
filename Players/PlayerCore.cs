using System;
using DarkTonic.MasterAudio;
using GameStates;
using Inputs;
using UniRx;
using UnityEngine;
using Zenject;
namespace Players
{
    public class PlayerCore : MonoBehaviour
    {
        [SerializeField] private GameObject leftDoor;
        [SerializeField] private Collider leftDoorDamageTrigger;

        [SerializeField] private GameObject rightDoor;
        [SerializeField] private Collider rightDoorDamageTrigger;
        
        [Inject] private IGameStateProvider _gameStateProvider;
        
        private static readonly int TriggerDoorOpen = Animator.StringToHash("TriggerDoorOpen");
        
        private IInputProvider _inputProvider;
        private Animator _leftDoorAnimator;
        private Animator _rightDoorAnimator;


        private void Awake()
        {
            _leftDoorAnimator = leftDoor.GetComponent<Animator>();
            _rightDoorAnimator = rightDoor.GetComponent<Animator>();

            leftDoorDamageTrigger.enabled = false;
            rightDoorDamageTrigger.enabled = false;

            _inputProvider = GetComponent<IInputProvider>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _gameStateProvider.CurrentState.Where(x => x == GameState.InGame).Subscribe(x =>
            {
                StartInput();
            }).AddTo(this);

        }

        private void StartInput()
        {
            _inputProvider.LeftMouseDown
                .Where(x => x == true)
                .Where(_ => _gameStateProvider.CurrentState.Value == GameState.InGame)
                .Subscribe(_ =>
                {

                    MasterAudio.PlaySound("door");

                    _leftDoorAnimator.SetTrigger(TriggerDoorOpen);
                    leftDoorDamageTrigger.enabled = true;
                    Observable.Timer(TimeSpan.FromSeconds(0.15f)).Subscribe(_ =>
                    {
                        leftDoorDamageTrigger.enabled = false;
                    }).AddTo(this);
                }).AddTo(this);

            _inputProvider.RightMouseDown
                .Where(x => x == true)
                .Where(_ => _gameStateProvider.CurrentState.Value == GameState.InGame)
                .Subscribe(_ =>
                {

                    MasterAudio.PlaySound("door");

                    _rightDoorAnimator.SetTrigger(TriggerDoorOpen);
                    rightDoorDamageTrigger.enabled = true;
                    Observable.Timer(TimeSpan.FromSeconds(0.3f)).Subscribe(_ =>
                    {
                        rightDoorDamageTrigger.enabled = false;
                    }).AddTo(this);
                }).AddTo(this);

        }
    }
}
