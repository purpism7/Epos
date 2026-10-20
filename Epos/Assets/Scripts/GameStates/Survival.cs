using UnityEngine;

using InputStates;

namespace GameStates
{
    public class Survival : GameState
    {
        public override GameState Create()
        {
            InputState = new FocusedInput();

            return this;
        }
    }
}

