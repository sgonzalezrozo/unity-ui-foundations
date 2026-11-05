using System;

namespace UIFoundations
{
    public interface IStateMachineTransition
    {
        void Show(StateObject state, Action onComplete);
        void Hide(StateObject state, Action onComplete);
        void Cancel();
    }
}


