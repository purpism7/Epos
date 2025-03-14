using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameSystem
{
    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private Light2D light2d = null;
        [SerializeField] private TextMeshProUGUI timeTMP = null;
        
        private float _dayLenght = 60f; // 하루의 길이(초).
        private float _dayIntensity = 1f;
        private float _nightIntensity = 0.4f;

        private Color _dayColor = Color.white;
        private Color _nightColor = new Color(30 / 255f, 130 / 255f, 255 / 255f);
        
        private float _timeOfDay = 1f; // 현재 시간 (0 - 1 범위로, 0은 낮 시작, 1은 다시 밤 시작)

        public bool IsNight { get { return _timeOfDay >= 0.5f; } }

        public void ChainUpdate()
        {
            if (light2d == null)
                return;

            _timeOfDay += Time.deltaTime / _dayLenght;
            if (_timeOfDay > 1f)
                _timeOfDay = 0;

            UpdateLighting();
            UpdateTime();
        }

        private void UpdateLighting()
        {
            if (light2d == null)
                return;
            
            float time = 0;
            float startIntensity = 1f;
            float endIntensity = 1f;
            Color startColor = Color.white;
            Color endColor = Color.white;
            
            if (_timeOfDay < 0.5f) // 낮
            {
                time = _timeOfDay * 2f;
                
                startIntensity = _nightIntensity;
                endIntensity = _dayIntensity;

                startColor = _nightColor;
                endColor = _dayColor;
            }
            else // 밤
            {
                time = (_timeOfDay - 0.5f) * 2;
                
                startIntensity = _dayIntensity;
                endIntensity = _nightIntensity;
                
                startColor = _dayColor;
                endColor = _nightColor;
            }
            
            light2d.intensity = Mathf.Lerp(startIntensity, endIntensity, time); 
            light2d.color = Color.Lerp(startColor, endColor, time); 
        }

        private void UpdateTime()
        {
            string dayNight = IsNight ? "Night" : "Day";

            float time = _timeOfDay * 24f;
            int hours = Mathf.FloorToInt(time);
            int minutes = (int)((time - hours) * 60);
            int seconds = (int)(((time - hours) * 60 - minutes) * 60f);
            timeTMP?.SetText( $"{dayNight} {hours}:{minutes}:{seconds}");
        }
    }
}    

