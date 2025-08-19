using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

using Spine;

namespace Creature.Action
{
    public class Move : Act<Move.Param>
    {
        public class Param : ActParam
        {
            public float MoveSpeed = 1f;
            public Transform TargetTm { get; private set; } = null;
            public ICombatant TargetICombatant { get; private set; } = null;
            public Vector3? TargetPos = null;
            public Vector3? OffsetPosition { get; private set; } = null;
            public bool ForwardDirection { get; private set; } = false;

            public System.Action FinishAction = null;
            public bool IsJumpMove = false;
            public bool UseNavMesh = true;
            
            public int DirectionAfterArriving = 1;

            public Param WithTargetTm(Transform targetTm)
            {
                TargetTm = targetTm;
                return this;
            }

            public Param WithTargetICombatant(ICombatant targetICombatant)
            {
                TargetICombatant = targetICombatant;
                return this;
            }

            public Param WithOffsetPosition(Vector3 position)
            {
                OffsetPosition = position;
                return this;
            }

            public Param WithForwardDirection(bool forwardDirection)
            {
                ForwardDirection = forwardDirection;
                return this;
            }
        }

        private Vector3 _prevPos = Vector3.zero;
        private Vector3 _targetPos = Vector3.zero;
        
        public bool IsJumpMove { get { return _param != null ? _param.IsJumpMove : false; } }

        public override void Execute()
        {
            if (_param == null)
                return;
            
            Activate();
            SetAnimation(_param.AnimationKey, true);

            _param.MoveSpeed *= 2f;
            _targetPos = CalcTargetPos;

            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent != null &&
                _param != null &&
                _param.UseNavMesh)
            {
                //var targetPos = TargetPos;
                //var distance = Vector2.Distance(navMeshAgent.transform.position, targetPos);
                //if (navMeshAgent.SamplePathPosition(NavMesh.AllAreas, distance, out NavMeshHit hit))
                //{
                //    Debug.DrawLine(navMeshAgent.transform.position, hit.position, Color.red);
                //}
                //var targetPos = CalcTargetPos;

                navMeshAgent.speed = _param.MoveSpeed;
                navMeshAgent.SetDestination(_targetPos);
                
                //_iActor?.NavMeshAgent.SamplePathPosition
            }

            if (_iActor?.Transform)
                _prevPos = _iActor.Transform.position;
        }

        protected override void Activate()
        {
            base.Activate();

            if (_iActor?.NavMeshAgent != null)
            {
                _iActor.NavMeshAgent.enabled = true;
                _iActor.NavMeshAgent.isStopped = false;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (_iActor?.NavMeshAgent != null)
            {
                _iActor.NavMeshAgent.isStopped = true;
                _iActor.NavMeshAgent.enabled = false;
            }
        }

        // private void Flip()
        // {
        //     var iActorTm = _iActor?.NavMeshAgent?.transform;
        //     if (!iActorTm)
        //         return;
        //
        //     // var rigidbody = _iActor.Rigidbody2D;
        //     // if (rigidbody == null)
        //     //     return;
        //     
        //     var direction = _param.TargetPos.x - iActorTm.position.x;
        //     iActorTm.localScale = new Vector3(direction < 0 ? -1f : 1f, 1f, 1f);
        // }

        private Vector3 TargetPos
        {
            get
            {
                Vector3 targetPos = Vector3.zero;
                if(_param != null)
                {
                    if (_param.TargetTm)
                        targetPos = _param.TargetTm.position;

                    if (_param.TargetPos != null)
                        targetPos = _param.TargetPos.Value;

                    if (_param.TargetICombatant != null)
                        targetPos = _param.TargetICombatant.Transform.position;
                }

                return targetPos;
            }
        }

        private Vector3 CalcTargetPos
        {
            get
            {
                if (_iActor == null)
                    return Vector3.zero ;

                Vector3 targetPos = TargetPos;
                var direction = targetPos - _iActor.Transform.position;
                Vector3 offsetPosition = Vector3.zero;
                float offsetDistance = 0;

                if (_param.OffsetPosition != null)
                {
                    offsetPosition = _param.OffsetPosition.Value;
                    offsetDistance = offsetPosition.x;

                    targetPos.x = direction.x <= 0 ? targetPos.x + offsetPosition.x : targetPos.x - offsetPosition.x;
                    targetPos.y += offsetPosition.y;
                }

                if (_param.ForwardDirection)
                    targetPos.x += direction.x;
                    //Debug.Log(direction);

                return targetPos;
            }
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (!_isActivate)
                return;
            
            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            if (_iActor?.IStat == null)
                return;

            if (_param?.TargetICombatant != null &&
                !_param.TargetICombatant.IsActivate)
            {
                End();
                return;
            }

            Vector3 targetPos = TargetPos;
            var direction = targetPos - iActorTm.position;
            Vector3 offsetPosition = Vector3.zero;
            float offsetDistance = 0;

            if (_param.OffsetPosition != null)
            {
                offsetPosition = _param.OffsetPosition.Value;
                offsetDistance = offsetPosition.x;

                targetPos.x = direction.x <= 0 ? targetPos.x + offsetPosition.x : targetPos.x - offsetPosition.x;
                targetPos.y += offsetPosition.y;
            }

            //var resTargetPos = CalcTargetPos;
            Debug.DrawLine(iActorTm.position, _targetPos, Color.blue);

            if (_param != null &&
              _param.UseNavMesh)
                UpdateMovementUsingNavMesh(_targetPos);
            else 
                UpdateMovementUsingTransform(iActorTm, _targetPos);

            direction = _prevPos - iActorTm.position;
            if (direction.x > 0)
                iActorTm.localScale = new Vector3(-1, 1, 1);
            else if (direction.x < 0)
                iActorTm.localScale = Vector3.one;

            _prevPos = iActorTm.position;

            var distance = Vector2.Distance(iActorTm.position, targetPos);
            if (distance < offsetDistance)
            {
                // 도착 후, 현재 바라보는 방향과 반대로 바라보기.
                var localScale = iActorTm.localScale;
                localScale.x = _param.DirectionAfterArriving;

                iActorTm.localScale = localScale;

                End();
            }
        }

        private void UpdateMovementUsingNavMesh(Vector3 targetPos)
        {
            //Vector3 targetPos = TargetPos;
            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent == null)
                return;

            if (!navMeshAgent.enabled)
                return;

            Debug.Log(navMeshAgent.hasPath);
            if (!navMeshAgent.pathPending && 
                navMeshAgent.remainingDistance <= 0.1f)
            {
                Debug.Log("타겟에 도착함");
                End();
            }
        }

        //private Vector2 StepToward(Vector2 current, Vector2 target, float step)
        //{
        //    var delta = target - current;
        //    var sqr = delta.sqrMagnitude;
        //    if (sqr < 1e-6f * 1e-6f) 
        //        return current; // 거의 같은 위치면 그대로

        //    var dir = delta / Mathf.Sqrt(sqr);   // normalized
        //    return current + dir * step;            // dir.normalized * move
        //}

        //private Vector2 MoveStraightTrig(Vector2 current, Vector2 target)
        //{
        //    var dir = target - current;
        //    var dist = dir.magnitude;
        //    if (dir.sqrMagnitude < 1e-8f) return target;

        //    float theta = Mathf.Atan2(dir.y, dir.x); // 라디안
        //    float dx = Mathf.Cos(theta) * dist;      // Mathf.Sin/Cos는 라디안 사용
        //    float dy = Mathf.Sin(theta) * dist;

        //    // 오버슈트 방지
        //    if (dist > dir.magnitude) return target;
        //    return current + new Vector2(dx, dy);
        //}

        private void UpdateMovementUsingTransform(Transform iActorTm, Vector3 targetPos)
        {
            var moveSpeed = _param.MoveSpeed;
            iActorTm.position = Vector2.MoveTowards(iActorTm.position, targetPos, moveSpeed * Time.deltaTime);
        }

        private void End()
        {
            _param.FinishAction?.Invoke();
            _endAction?.Invoke(_iActor);
        }
    }
}
