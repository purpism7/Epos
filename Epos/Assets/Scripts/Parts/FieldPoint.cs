using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Common;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Creature;

using Vector3 = UnityEngine.Vector3;

namespace Parts
{
    public interface IFieldPoint
    {
        void Initialize();
        void Activate();
        void Deactivate();
        void ChainUpdate();

        Transform PointTm { get; }
    }

    public class FieldPoint : Part, IFieldPoint
    {
        [SerializeField]
        private Transform pointTm = null;

        [SerializeField] 
        private Deploy leftDeploy = null;
        [SerializeField] 
        private Deploy rightDeploy = null;
        
        // 임시
        [SerializeField]
        private Monster monster = null;

        Collider2D[] _colliders = new Collider2D[5];

        public Transform PointTm { get { return pointTm; } }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!pointTm)
                return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointTm.position, 5f);
        }
#endif

        #region FieldPoint
        public override void Initialize()
        {
            base.Initialize();
            
            monster?.Initialize();
            
            leftDeploy?.SetActive(false);
            rightDeploy?.SetActive(false);
        }
        
        public override void Activate()
        {
            base.Activate();
            
            RandomActionAsync().Forget();
        }
        
        public void ChainUpdate()
        {
            if (!IsActivate)
                return;

            if (monster == null)
                return;
            
            monster.ChainUpdate();
            
            if (Physics2D.OverlapCircleNonAlloc(monster.Transform.position, 3f, _colliders) > 0)
            {
                foreach (var collider in _colliders)
                {
                    if(collider == null)
                        continue;

                    var hero = collider.GetComponentInParent<Hero>();
                    if (hero != null)
                    {
                        Array.Clear(_colliders, 0, _colliders.Length);
                        
                        return;
                    }
                }
            }
        }
        #endregion

        private async UniTask RandomActionAsync()
        {
            await UniTask.WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
            
            if (!IsActivate)
                return;
            
            if (!pointTm)
                return;
            
            if (monster == null)
                return;

            float value = 5f;
            float randomX = UnityEngine.Random.Range(-value, value);
            float randomY = UnityEngine.Random.Range(-value, value);
            
            var targetPos = new Vector3(pointTm.position.x + randomX, pointTm.position.y + randomY, 0);
            monster.IActCtr?.MoveToTarget("02_Run", targetPos,
                () =>
                {
                    RandomActionAsync().Forget();
                });
        }
    }
}

