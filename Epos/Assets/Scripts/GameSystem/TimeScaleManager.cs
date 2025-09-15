using Entities;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace GameSystem
{
    public interface ITimeScaleManager
    {
        void Resume();
        void Pause();

        void Set(float timeScale);
    }

    public class TimeScaleManager : ITimeScaleManager
    {
        private float _timeScale = 0;

        void ITimeScaleManager.Resume()
        {
            Time.timeScale = _timeScale;
        }

        void ITimeScaleManager.Pause()
        {
            Time.timeScale = 0;
        }

        void ITimeScaleManager.Set(float timeScale)
        {
            Time.timeScale = timeScale;

            _timeScale = timeScale;
            //Time.fixedDeltaTime = 0.02f * timeScale;
        }        
    }
}
