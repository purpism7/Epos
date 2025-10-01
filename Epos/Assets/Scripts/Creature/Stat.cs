using Cysharp.Threading.Tasks;
using GameSystem.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Creature
{
    public interface IStatGeneric
    {
        void Initialize(Character character);
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

        private bool _isUpdateMp = false;

        private bool _isActivate = false;

        #region IStatGeneric
        void IStatGeneric.Initialize(Character character)
        {
            _iListener = character;
            _isUpdateMp = false;
        }
        
        void IStatGeneric.Activate()
        {
            _isActivate = true;
        }

        void IStatGeneric.Deactivate()
        {
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
                _originStatDic.Clear();
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
                _addedStatDic.Clear();
            }

            if (!_addedStatDic.TryGetValue(eType, out var subDic) || subDic == null)
            {
                subDic = new Dictionary<ESubType, float>();
                _addedStatDic?.TryAdd(eType, subDic);
            }

            if (subDic.ContainsKey(eSubType))
                subDic[eSubType] += value;
            else
                subDic[eSubType] = value;

            _iListener?.OnStatChanged(eType, GetCurrent(eType));

            if (eType == EType.Mp &&
                !_isUpdateMp)
                UpdateMpAsync().Forget();
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

        private async UniTask UpdateMpAsync()
        {
            if (!_isActivate)
                return;

            if (_isUpdateMp)
                return;

            var maxMp = GetCurrent(EType.MaxMp);
            var mp = maxMp - GetCurrent(EType.Mp);
            if (mp <= 0)
                return;

            _isUpdateMp = true;

            float regenRate = 1f / 1f;

            while (maxMp - GetCurrent(EType.Mp) > 0)
            {
                if (!_isActivate)
                    break;

                maxMp = GetCurrent(EType.MaxMp);
                mp = Time.deltaTime * regenRate;

                if (mp + GetCurrent(EType.Mp) > maxMp)
                {
                    SetAdded(EType.Mp, ESubType.None, 0);
                    break;
                }

                SetAdded(EType.Mp, ESubType.None, mp);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            _isUpdateMp = false;
        }
    }
}

