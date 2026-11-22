using System.Collections.Generic;
using UnityEngine;

using Actor;

namespace Characters.Actions
{
    public interface IActionController
    {
        void Flip(float x);
        // void MoveByAxis(Vector2 moveAxis);
        void HandleMoveInput(Vector2 moveAxis);

        /// <summary>마지막으로 받은 이동 입력(조작 의도). BattleManager 가 매 프레임 읽어 전투 시뮬에 넘긴다.</summary>
        Vector2 MoveInput { get; }

        // 전투 표현용(BattleUnitPresenter 가 시뮬 상태가 바뀔 때 부른다). 위치 · 피해 · 맞는 때는 시뮬 몫이고 여기서는 그림만 고른다.
        // faceDirection 쪽을 먼저 본 뒤 한 번만 튼다.

        /// <summary>대기 그림으로 바꾼다. 공격 그림이 아직 도는 중이면 바꾸지 않고 false(부른 쪽이 다음 프레임에 다시 부른다).</summary>
        bool TryShowIdle();
        void ShowMove(Vector2 faceDirection);
        void ShowChase(Vector2 faceDirection);
        /// <summary>같은 공격 액션이 돌고 있어도 처음부터 다시 튼다(새 휘두르기마다).</summary>
        void ShowAttack(Vector2 faceDirection);
    }
    
    public class ActionController : IActionController
    {
        private IActor _actor = null;
        private Action _currentAction = null;

        // 마지막 이동 입력(조작 의도). 실제 이동은 전투 시뮬이 한다.
        private Vector2 _moveInput = Vector2.zero;

        // 전투 표현이 상태가 바뀔 때마다 부르므로 매개변수를 하나씩 만들어 두고 다시 쓴다(부를 때마다 new 하지 않는다).
        private readonly MoveParam _moveParam = new MoveParam();
        private readonly ChaseParam _chaseParam = new ChaseParam();
        private readonly AttackParam _attackParam = new AttackParam();
        
        private Dictionary<System.Type, Action> _actions = new Dictionary<System.Type, Action>();

        public ActionController(IActor actor)
        {
            _actions?.Clear();
            
            _actor = actor;
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            
        }

        public void ChainUpdate()
        {
            _currentAction?.ChainUpdate();
        }
        
        #region IActionController

        void IActionController.Flip(float x)
        {
            // 좌우 반전은 CharacterAnimator 한 곳에서만 한다(그림 기본 방향 반영, 문턱 0.01 동일, 앞뒤는 그대로).
            _actor?.CharacterAnimator?.Face(new Vector2(x, 0f));
        }

        Vector2 IActionController.MoveInput => _moveInput;

        void IActionController.HandleMoveInput(Vector2 moveAxis)
        {
            // 입력은 조작 의도로만 적어 둔다. 실제 이동은 전투 시뮬이 고정 틱으로 하고(BattleManager 가 이 값을 넘긴다),
            // 달리기 · 대기 그림은 그 결과를 따라 BattleUnitPresenter 가 고른다.
            _moveInput = moveAxis;
        }

        bool IActionController.TryShowIdle()
        {
            // 시뮬이 휘두르기를 끝냈어도 공격 그림이 아직 도는 중이면 끝까지 보여 준다
            // (데이터의 동작 길이와 그림 길이가 달라도 휘두르는 그림이 잘리지 않게). 이동 · 추격 · 새 휘두르기는 기다리지 않는다.
            if (_currentAction is Attack attack && attack.IsSwingPlaying)
                return false;

            ExecuteAction<Idle, IdleParam>();
            return true;
        }

        void IActionController.ShowMove(Vector2 faceDirection)
        {
            _moveParam.MoveAxis = faceDirection;
            ExecuteAction<Move, MoveParam>(_moveParam);
        }

        void IActionController.ShowChase(Vector2 faceDirection)
        {
            _chaseParam.FaceDirection = faceDirection;
            ExecuteAction<Chase, ChaseParam>(_chaseParam);
        }

        void IActionController.ShowAttack(Vector2 faceDirection)
        {
            _attackParam.FaceDirection = faceDirection;
            ExecuteAction<Attack, AttackParam>(_attackParam, restart: true);
        }
        #endregion

        private void ExecuteAction<TAction, TParam>(TParam param = null, bool restart = false)
            where TAction : Action<TParam>, new()
            where TParam : ActionParam
        {
            if (_actions == null)
                _actions = new();
            
            var actionKey = typeof(TAction);
            if (!_actions.TryGetValue(actionKey, out var action))
            {
                action = new TAction();
                action.SetActor(_actor)?
                    .SetEndAction(EndAction);
                
                _actions[actionKey] = action;
            }

            action.SetActionParam(param);
            
            // 같은 액션이면 Execute 를 다시 부르지 않는다. restart 면 같은 액션이어도 처음부터 다시 한다(연달아 시작되는 휘두르기).
            if (restart || _currentAction?.GetType() != typeof(TAction))
                action.Execute();

            _currentAction = action;
        }

        private void EndAction()
        {
            if (_currentAction is Move)
                ExecuteAction<Idle, IdleParam>();
        }
    }
}

