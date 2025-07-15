using Cysharp.Threading.Tasks;
using Datas.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        IActController MoveToTargetPosition(Move.Data data);
        //IActController MoveToTargetPosition(float moveSpeed, Vector3? pos = null, System.Action finishAction = null, int direction = 1, bool isJumpMove = false, bool useNavMesh = true);
        IActController MoveToTarget(Move.Data data);
        IActController CastingSkill(Casting.IListener iListener, ICombatant iCombatant, Skill skill, List<ICombatant> targetList);
        IActController Die();
        
        void TakeDamage(ICaster iCaster, bool playAnimation);
        void Execute();

        bool InAction { get; }

        void SetPosition(Vector3 position);
    }
    
    public class ActController : Controller, IActController
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _iActQueue = null;
        private Vector3 _currPosition = Vector3.zero;

        public bool InAction { get; private set; } = false;

        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iActDic = new();
            _iActDic.Clear();
            
            return this;
        }

        #region IController

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
            Idle();
        }
        #endregion
            
        #region IActController
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="finishAction"></param>
        /// <param name="reverse">Target Pos 에 도착 후, 반대 방향으로 Flip 할지.</param>
        /// <returns></returns>
        IActController IActController.MoveToTargetPosition(Move.Data moveData)
        {
            if (!IsActivate)
                return null;

            if (moveData == null)
                return null;

            var targetPos = _currPosition;
            if (moveData.TargetPos != null)
                targetPos = moveData.TargetPos.Value;
         
            AddActAsync<Move, Move.Data>(moveData).Forget();

            return this;
        }

        IActController IActController.MoveToTarget(Move.Data moveData)
        {
            if (!IsActivate)
                return null;

            if (moveData == null)
                return null;

            AddActAsync<Move, Move.Data>(moveData).Forget();

            return this;
        }

        IActController IActController.CastingSkill(Casting.IListener iListener, ICombatant iCombatant, Skill skill, List<ICombatant> targetList)
        {
            if (!IsActivate)
                return null;
            
            var data = new Casting.Data
            {
                IListener = iListener,
                ICombatant = iCombatant,
                Skill = skill,
                TargetList = targetList,
            };
            
            AddActAsync<Casting, Casting.Data>(data).Forget();

            return this;
        }

        IActController IActController.Die()
        {
            Execute<Die, Die.Data>();
            
            return this;
        }

        void IActController.TakeDamage(ICaster iCaster, bool PlayAnimation)
        {
            if (!IsActivate)
                return;

            var data = new Damage.Data
            {
                ICaster = iCaster,
                PlayAnimation = PlayAnimation,
            };
            
            Execute<Damage, Damage.Data>(data);
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
                    
                    iAct?.Execute();
                    SetCurrIAct(iAct);
                    
                    return;
                }
            }

            Idle();
        }

        public void Execute()
        {
            ExecuteAsync().Forget();
        }

        private void Idle()
        {
            Execute<Idle, Idle.Data>();
            SetCurrIAct(null);
            
            InAction = false;
        }

        private async UniTask AddActAsync<T, V>(V data = null) where T : Act<V>, new() where V : Act<V>.BaseData, new()
        {
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (data == null)
                data = new V();
            
            act.SetData(data);
            var animationKey = _iActor?.AnimationKey(act);
            data.SetAnimationKey(animationKey); 
            
            if (_iActQueue == null)
            {
                _iActQueue = new();
                _iActQueue.Clear();
            }
            
            _iActQueue?.Enqueue(act);
        }

        private Act<V> GetAct<T, V>() where T : Act<V>, new() where V : Act<V>.BaseData, new()
        {
            if (_iActDic == null)
            {
                _iActDic = new();
                _iActDic.Clear();
            }
            
            System.Type type = typeof(T);
            Act<V> act = null;
            
            if (_iActDic.TryGetValue(type, out IAct iAct))
            {
                act = iAct as Act<V>;
            }
            else
            {
                act = new T();
                act.Initialize(_iActor);
                act.SetEndActAction(EndAct);
                
                _iActDic?.TryAdd(type, act);
            }
            
            return act;
        }
        
        void IActController.SetPosition(Vector3 position)
        {
            _currPosition = position;
            _currPosition.z = 0;
        }
        #endregion

        private void Execute<T, V>(V data = null) where T : Act<V>, new() where V : Act<V>.BaseData, new()
        {
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (data == null)
                data = new V();

            data.SetAnimationKey(_iActor?.AnimationKey(act));
            
            act.SetData(data);
            act.Execute();
            
            SetCurrIAct(act);
        }
        
        private void EndAct()
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

