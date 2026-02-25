using Cysharp.Threading.Tasks;
using GameSystem.Event;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Creature
{
    public interface IStatGeneric
    {
        void Initialize(Character character);
        void Dispose();
        void Activate();
        void Deactivate();

        Stat Stat { get; }
    }
    
    public interface IStat
    {
        void SetOrigin(Stat.EType eStatType, float value);
        void Add(Stat.EType eStatType, Stat.ESubType eSubType, float value);

        float Get(Stat.EType eStatType);
    }
    
    public class Stat : IStatGeneric, IStat
    {
        public enum EType
        {
            None,
            
            ActionSpeed,
            
            ActivePoint,
            PassivePoint,
            
            Attack,
            AttackRange,
            MoveSpeed,
            
            Hp,
            MaxHp,

            Mp,
            MaxMp,
            
            AttackSight,
        }

        public enum ESubType
        {
            None,

            Hp,

            Fatigue,
        }

        public interface IListener
        {
            void OnStatChanged(EType eType, float value);
        }
        
        private IListener _iListener = null;
        private Dictionary<EType, float> _originStatDic = new();
        private Dictionary<EType, Dictionary<ESubType, float>> _addedStatDic = new();
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private bool _isUpdateMp = false;

        private bool _isActivate = false;

        #region IStatGeneric
        void IStatGeneric.Initialize(Character character)
        {
            _iListener = character;
            _isUpdateMp = false;
        }

        public void Dispose()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();  // 돌고 있는 UpdateMpAsync 루프를 강제 중단!
                _cancellationToken.Dispose(); // 토큰 메모리 해제
                _cancellationToken = null;
            }
        }
        
        void IStatGeneric.Activate()
        {
            if (_cancellationToken == null)
                _cancellationToken = new CancellationTokenSource();

            _isActivate = true;
        }

        void IStatGeneric.Deactivate()
        {
            Dispose();

            _isActivate = false;
        }

        Stat IStatGeneric.Stat
        {
            get { return this; }
        }
        #endregion
        
        #region IStat
        void IStat.SetOrigin(EType eType, float value)
        {
            SetOrigin(eType, value);
        }
        
        void IStat.Add(EType eType, ESubType eSubType, float value)
        {
            SetAdded(eType, eSubType, value);
        }

        float IStat.Get(EType eType)
        {
            return GetCurrent(eType);
        }
        #endregion

        private float GetCurrent(EType eType)
        {
            return GetOrigin(eType) + GetAdded(eType);
        }

        private void SetOrigin(EType eType, float value)
        {
            if (_originStatDic == null)
            {
                _originStatDic = new();
            }

            if (_originStatDic.ContainsKey(eType))
                _originStatDic[eType] = value;
            else
                _originStatDic.TryAdd(eType, value);
        }
        
        private void SetAdded(EType eType, ESubType eSubType, float value)
        {
            if (_addedStatDic == null)
            {
                _addedStatDic = new();
            }

            if (!_addedStatDic.TryGetValue(eType, out var subDic) || subDic == null)
            {
                subDic = new Dictionary<ESubType, float>();
                _addedStatDic[eType] = subDic;
            }

            if (subDic.ContainsKey(eSubType))
                subDic[eSubType] += value;
            else
                subDic[eSubType] = value;

            _iListener?.OnStatChanged(eType, GetCurrent(eType));

            if (eType == EType.Mp &&
                !_isUpdateMp)
            {
                if (_cancellationToken != null)
                    UpdateMpAsync(_cancellationToken.Token).Forget();
            }  
        }

        private float GetOrigin(EType eType)
        {
            if (_originStatDic == null)
                return 0;
            
            if (_originStatDic.TryGetValue(eType, out float value))
                return value;

            return 0;
        }
        
        private float GetAdded(EType eType)
        {
            if (_addedStatDic == null)
                return 0;

            float value = 0;
            if (_addedStatDic.TryGetValue(eType, out var subDic))
            {
                foreach(var keyValuePair in subDic)
                {
                    value += keyValuePair.Value;
                }
            }

            return value;
        }

        // 💡 호출하는 곳(Start 등)에서 this.GetCancellationTokenOnDestroy() 를 넘겨주세요.
        private async UniTask UpdateMpAsync(CancellationToken cancellationToken)
        {
            // 1. 방어 로직 간소화
            if (!_isActivate || _isUpdateMp)
                return;

            var maxMp = GetCurrent(EType.MaxMp);
            var currentMp = GetCurrent(EType.Mp);

            if (currentMp >= maxMp)
                return;

            _isUpdateMp = true;

            // 초당 회복량 (예: 1초에 1씩 회복)
            float regenRate = 1f;

            // 2. 객체 파괴 시 에러가 나지 않도록 CancellationToken 체크 추가
            while (_isActivate && !cancellationToken.IsCancellationRequested)
            {
                maxMp = GetCurrent(EType.MaxMp);
                currentMp = GetCurrent(EType.Mp);

                // 루프 도중 최대치에 도달했다면 종료
                if (currentMp >= maxMp)
                    break;

                // 이번 프레임에 더해질 MP 회복량
                float frameRegenAmount = Time.deltaTime * regenRate;

                // 3. 💡 버그 수정: 더했을 때 최대치를 초과한다면, 딱 '모자란 만큼'만 더해서 꽉 채워줌
                if (currentMp + frameRegenAmount >= maxMp)
                {
                    float amountToMax = maxMp - currentMp;
                    SetAdded(EType.Mp, ESubType.None, amountToMax);
                    break;
                }

                // 최대치를 넘지 않는다면 정상적으로 회복량 추가
                SetAdded(EType.Mp, ESubType.None, frameRegenAmount);

                // 다음 프레임까지 대기 (취소 토큰 전달)
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _isUpdateMp = false;
        }
    }
}

