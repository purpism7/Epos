using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using Datas.ScriptableObjects;
using Creator;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        IActController MoveToTargetPosition(Move.Param param);
        IActController MoveToTarget(Move.Param param);
        IActController CastingSkill(Casting.IListener iListener, ICombatant iCombatant, Ability.ISkill iSkill, ICombatant target, List<ICombatant> targetList);

        IActController Knockback(Knockback.Param param);
        IActController Die();
        IActController Victory();

        void Impact(IStat iStat, Common.EImpactType eImpactType, bool playAnimation);

        void Execute();

        bool InAction { get; }

        void Flip(float x);
        void SetPosition(Vector3 position);
    }
    
    public class ActController : Controller, IActController
    {
        [Inject] private IObjectResolver _iResolver = null;

        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _iActQueue = null;
        private Vector3 _currPosition = Vector3.zero;

        public bool InAction { get; private set; } = false;

        #region IController
        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iActDic = new();
            _iActDic.Clear();

            Preload();

            return this;
        }

        void IController<IActController, IActor>.ChainUpdate()
        {
            if (!IsActivate)
                return;
            
            _currIAct?.ChainUpdate();
        }
        
        void IController<IActController, IActor>.ChainFixedUpdate()
        {
            if (!IsActivate)
                return;
            
            _currIAct?.ChainFixedUpdate();
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            _iActQueue?.Clear();
            //Idle();
        }
        #endregion
            
        private void Preload()
        {
            //GetAct<Casting, Casting.Param>();
        }

        #region IActController
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="finishAction"></param>
        /// <param name="reverse">Target Pos 에 도착 후, 반대 방향으로 Flip 할지.</param>
        /// <returns></returns>
        IActController IActController.MoveToTargetPosition(Move.Param moveParam)
        {
            if (!IsActivate)
                return null;

            if (moveParam == null)
                return null;

            AddActAsync<Move, Move.Param>(moveParam).Forget();

            return this;
        }

        IActController IActController.MoveToTarget(Move.Param moveParam)
        {
            if (!IsActivate)
                return null;

            if (moveParam == null)
                return null;

            AddActAsync<Move, Move.Param>(moveParam).Forget();

            return this;
        }

        IActController IActController.CastingSkill(Casting.IListener iListener, ICombatant iCaster, Ability.ISkill iSkill, ICombatant target, List<ICombatant> targetList)
        {
            if (!IsActivate)
                return null;

            var castingParam = new Casting.Param
            {
                IListener = iListener,
                ISkill = iSkill,
            }
            .WithAttacker(iCaster)
            .WithTarget(target)
            .WithTargetList(targetList);
            
            AddActAsync<Casting, Casting.Param>(castingParam).Forget();

            return this;
        }

        IActController IActController.Knockback(Knockback.Param param)
        {
            if (!IsActivate)
                return null;

            Execute<Knockback, Knockback.Param>(param);
            
            return this;
        }

        IActController IActController.Die()
        {
            Execute<Die, Die.Param>();
            
            return this;
        }

        IActController IActController.Victory()
        {
            Execute<Victory, Victory.Param>();

            return this;
        }

        void IActController.Impact(IStat iStat, Common.EImpactType eImpactType, bool PlayAnimation)
        {
            if (!IsActivate)
                return;

            var impactParam = new Impact.Param
            {
                PlayAnimation = PlayAnimation,
            }
            .WithIStat(iStat)
            .WithEImpactType(eImpactType);
            
            Execute<Impact, Impact.Param>(impactParam, false);
        }

        private async UniTask ExecuteAsync()
        {
            if (InAction)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            if (_iActQueue?.Count > 0)
            {
                if (_iActQueue.TryDequeue(out IAct iAct))
                {
                    InAction = true;

                    _currIAct?.Deactivate();
                    iAct?.Execute();
                    
                    SetCurrIAct(iAct);
                    
                    return;
                }
            }

            Idle();
        }

        public void Execute()
        {
            if (!IsActivate)
                return;
            
            ExecuteAsync().Forget();
        }

        private void Idle()
        {
            _currIAct?.Deactivate();
            
            Execute<Idle, Idle.Param>();
            SetCurrIAct(null);
            
            InAction = false;
        }

        private async UniTask AddActAsync<T, V>(V param = null) where T : Act<V>, new() where V : ActParam, new()
        {
            if (_currIAct is Die)
                return;
            
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (param == null)
                param = new V();
            
            act.SetParam(param);
            var animationKey = _iActor?.AnimationKey(act);
            param.SetAnimationKey(animationKey); 
            
            if (_iActQueue == null)
            {
                _iActQueue = new();
                _iActQueue.Clear();
            }
            
            _iActQueue?.Enqueue(act);
        }

        private Act<V> GetAct<T, V>() where T : Act<V>, new() where V : ActParam, new()
        {
            if (_iActDic == null)
            {
                _iActDic = new();
                _iActDic.Clear();
            }
            
            System.Type type = typeof(T);
            Act<V> act = null;
            
            if (_iActDic.TryGetValue(type, out IAct iAct))
                act = iAct as Act<V>;
            else
            {
                act = new T();
                _iResolver?.Inject(act);

                act.Initialize(_iActor);
                act.SetEndActAction(EndAct);
                
                _iActDic?.TryAdd(type, act);
            }
            
            return act;
        }

        void IActController.Flip(float x)
        {
            if (!IsActivate)
                return;

            var skeletonAnimation = _iActor?.SkeletonAnimation;
            if (skeletonAnimation == null)
                return;

            if (Mathf.Abs(x) > 0.01f)
                skeletonAnimation.Skeleton.ScaleX = x > 0 ? -1f : 1f;
        }

        void IActController.SetPosition(Vector3 position)
        {
            _currPosition = position;
            _currPosition.z = 0;
        }
        #endregion

        private void Execute<T, V>(V param = null, bool isSet = true) where T : Act<V>, new() where V : ActParam, new()
        {
            if (_currIAct is Die)
                return;

            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (param == null)
                param = new V();

            param.SetAnimationKey(_iActor?.AnimationKey(act));
            
            act.SetParam(param);
            act.Execute();

            if(isSet)
                SetCurrIAct(act);
        }
        
        private void EndAct(IActor iActor)
        {
            Execute();
        }

        private void SetCurrIAct(IAct iAct)
        {
            _currIAct = iAct;
            
            // Debug.Log(name + " = " + iAct?.GetType());
        }
    }
}

