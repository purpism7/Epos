using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Creature;
using GameSystem;
using Battle.Mode;
using Battle.Step;

namespace Parts
{
    public class FieldPoint : Part<FieldPoint.Data>
    {
        [SerializeField] 
        private int id = 0;
        [SerializeField]
        private Transform pointTm = null;

        [SerializeField] 
        private Forces leftForces = null;
        [SerializeField] 
        private Forces rightForces = null;
        
        // 임시
        [SerializeField]
        private Monster monster = null;

        private const float Range = 5f;

        public class Data : BaseData
        {
            public IListener IListener = null;
        }
        
        public interface IListener
        {
            void Encounter(int fieldPointId, IActor iActor); // 필드 영웅과 몬스터가 대치.
        }

        Collider2D[] _colliders = new Collider2D[5];

        public Transform PointTm { get { return pointTm; } }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!pointTm)
                return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointTm.position, Range);
        }
#endif

        #region FieldPoint
        public override Part<Data> Initialize(Data data)
        {
            base.Initialize(data);
            
            monster?.Initialize();
            
            leftForces?.Deactivate();
            rightForces?.Deactivate();

            return this;
        }
        
        public override void Activate()
        {
            base.Activate();
            
            monster?.Activate();
            
            RandomActionAsync().Forget();
        }
        
        public void ChainUpdate()
        {
            if (!IsActivate)
                return;

            if (monster == null ||
                !monster.IsActivate)
                return;
            
            monster.ChainUpdate();
            
            if (Physics2D.OverlapCircleNonAlloc(monster.Transform.position, Range, _colliders) > 0)
            {
                foreach (var collider in _colliders)
                {
                    if(collider == null)
                        continue;

                    var hero = collider.GetComponentInParent<Hero>();
                    if (hero != null)
                    {
                        _data?.IListener?.Encounter(id, hero);
                        
                        BeginFieldBattle();
                        
                        hero.Deactivate();
                        monster.Deactivate();
              
                        return;
                    }
                }
                
                Array.Clear(_colliders, 0, _colliders.Length);
            }
        }
        #endregion
        
        private void BeginFieldBattle()
        {
            var iBattleMgr = MainManager.Get<IBattleManager>();
            if (iBattleMgr == null)
                return;

            iBattleMgr.BeginFieldBattle(leftForces, rightForces, pointTm);
        }

        private async UniTask RandomActionAsync()
        {
            await UniTask.WaitForSeconds(UnityEngine.Random.Range(Range, Range * 2f));
            
            if (!IsActivate)
                return;
            
            if (!pointTm)
                return;
            
            if (monster == null ||
                !monster.IsActivate)
                return;

            float value = Range;
            float randomX = UnityEngine.Random.Range(-value, value);
            float randomY = UnityEngine.Random.Range(-value, value);
            
            var targetPos = new Vector3(pointTm.position.x + randomX, pointTm.position.y + randomY, 0);
            monster.IActCtr?.MoveToTarget(targetPos,
                () =>
                {
                    RandomActionAsync().Forget();
                }).Execute();
        }
    }
}

