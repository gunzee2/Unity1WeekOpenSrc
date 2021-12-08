using UniRx;
namespace Inputs
{
    public interface IInputProvider 
    {
        IReadOnlyReactiveProperty<bool> LeftMouseDown { get; }
        IReadOnlyReactiveProperty<bool> RightMouseDown { get; }
    }
}
