using UnityEngine;

namespace Creature.Emotion
{
    public abstract class BaseEmotion<T> where T : new()
    {
        private static T _instance = default;

        public static T Create()
        {
            if (_instance == null)
                _instance = new();

            return _instance;
        }

        // data 로 뺄 것.
        public string ImageName()
        {
            return $"Img_Battle_{typeof(T).Name}";
        }
    }
}

