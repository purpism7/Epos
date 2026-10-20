using UnityEngine;

using InputStates;

namespace GameStates
{
    public abstract class GameState
    {
        public IInputState InputState { get; protected set; } = null;

        public abstract GameState Create();
    }
}

