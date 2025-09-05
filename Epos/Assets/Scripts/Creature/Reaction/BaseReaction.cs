using UnityEngine;

namespace Creature.Reaction
{
    public abstract class BaseReaction<T> where T : new()
    {
        private static T _instance = default;

        public static T Create()
        {
            if (_instance == null)
                _instance = new();

            return _instance;
        }

        public abstract string ImageName();
    }
}

