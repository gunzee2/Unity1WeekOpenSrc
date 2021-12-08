using UniRx;
using UnityEngine;
namespace Inputs
{
    public class MouseInputPresenter : MonoBehaviour, IInputProvider
    {
        public IReadOnlyReactiveProperty<bool> LeftMouseDown => _leftMouseDown;
        public IReadOnlyReactiveProperty<bool> RightMouseDown => _rightMouseDown;

        private readonly ReactiveProperty<bool> _leftMouseDown = new ReactiveProperty<bool>();
        private readonly ReactiveProperty<bool> _rightMouseDown = new ReactiveProperty<bool>();


        // Start is called before the first frame update
        void Start()
        {
            this
                .ObserveEveryValueChanged(x => Input.GetMouseButtonDown(0))
                .Subscribe(x => _leftMouseDown.Value = x)
                .AddTo(this);
            this
                .ObserveEveryValueChanged(x => Input.GetMouseButtonDown(1))
                .Subscribe(x => _rightMouseDown.Value = x)
                .AddTo(this);
        }
    }
}
