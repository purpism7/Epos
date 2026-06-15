using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Scene
{
    public class TitleScene : MonoBehaviour
    {
        private const string TitleSceneName = "Title";
        private const string TouchToStartMessage = "Touch To Start";

        [SerializeField] private TMP_Text touchToStartText = null;
        [SerializeField] private float touchActivationDelay = 3f;
        [SerializeField] private float touchFadeDuration = 3f;
        [SerializeField] private string nextSceneName = "WorldMap";

        private Tween _touchFadeTween = null;
        private bool _canStart = false;
        private bool _isTransitioning = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoaded()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeCurrentScene()
        {
            EnsureTitleSceneController(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode loadSceneMode)
        {
            EnsureTitleSceneController(scene);
        }

        private static void EnsureTitleSceneController(UnityEngine.SceneManagement.Scene scene)
        {
            if (scene.name != TitleSceneName)
                return;

            var rootGameObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                var rootGameObject = rootGameObjects[i];
                if (!rootGameObject)
                    continue;

                var titleScene = rootGameObject.GetComponentInChildren<TitleScene>(true);
                if (titleScene)
                {
                    titleScene.enabled = true;
                    return;
                }

                var texts = rootGameObject.GetComponentsInChildren<TMP_Text>(true);
                for (int j = 0; j < texts.Length; ++j)
                {
                    var text = texts[j];
                    if (!text || text.text != TouchToStartMessage)
                        continue;

                    rootGameObject.AddComponent<TitleScene>();
                    return;
                }
            }
        }

        private void Awake()
        {
            if (!touchToStartText)
                touchToStartText = FindTouchToStartText();

            if (touchToStartText)
            {
                touchToStartText.alpha = 0f;
                touchToStartText.gameObject.SetActive(false);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(touchActivationDelay);

            ShowTouchToStart();
        }

        private void OnDestroy()
        {
            _touchFadeTween?.Kill();
        }

        private void Update()
        {
            if (!_canStart || _isTransitioning || !IsStartPressed())
                return;

            _isTransitioning = true;
            _canStart = false;
            _touchFadeTween?.Kill();
            touchToStartText?.gameObject.SetActive(false);

            LoadSceneManager.Instance.LoadSceneAsync(nextSceneName).Forget();
        }

        private void ShowTouchToStart()
        {
            if (!touchToStartText)
                return;

            touchToStartText.alpha = 0f;
            touchToStartText.gameObject.SetActive(true);

            if (touchFadeDuration <= 0f)
            {
                touchToStartText.alpha = 1f;
                _canStart = true;
                return;
            }

            _touchFadeTween?.Kill();
            _touchFadeTween = touchToStartText
                .DOFade(1f, touchFadeDuration)
                .SetEase(Ease.OutSine)
                .SetUpdate(true)
                .OnComplete(() => _canStart = true);
        }

        private static bool IsStartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Pointer.current?.press.wasReleasedThisFrame == true)
                return true;
#endif

            if (Input.GetMouseButtonUp(0))
                return true;

            for (int i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).phase == UnityEngine.TouchPhase.Ended)
                    return true;
            }

            return false;
        }

        private TMP_Text FindTouchToStartText()
        {
            var texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; ++i)
            {
                var text = texts[i];
                if (text && text.text == TouchToStartMessage)
                    return text;
            }

            return null;
        }
    }
}
