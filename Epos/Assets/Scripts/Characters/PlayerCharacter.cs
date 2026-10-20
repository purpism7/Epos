using UnityEngine;

namespace Characters
{
    public class PlayerCharacter : Character
    {
        public override void Initialize()
        {
            base.Initialize();
            
            Debug.Log("PlayerCharacter Initialize");
        }
    }
}

