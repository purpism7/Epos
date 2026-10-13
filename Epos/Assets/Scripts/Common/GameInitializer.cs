using UnityEngine;

using Managers;

namespace Common
{
    public class GameInitializer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnBeforeSceneStart()
        {
            // GameManager 생성.
            var gameManager = GameObject.FindAnyObjectByType<GameManager>();
            if (gameManager == null)
            {
                GameManager prefab = Resources.Load<GameManager>(nameof(GameManager));
                if (prefab == null)
                    return;

                Object.Instantiate(prefab);
            }
        }
    }
}

