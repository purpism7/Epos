using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using VContainer;
using Cysharp.Threading.Tasks;

using Common;
using Creator;
using Creature;
using Creature.Action;
using GameSystem;
using UI.Parts;


namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data> , WeightedActionExecutor.IListener, IWeightedActionRequester
    {
        public class Data : BaseData
        {
            public WayPoint[] WayPoints { get; private set; } = null;
            //public List<> WayPointTms { get; private set; } = null;

            public Data WithWayPoints(WayPoint[] wayPoints)
            {
                WayPoints = wayPoints;
                return this;
            }
        }

        [Inject] private ICameraManager _iCameraManager = null;
        
        private Queue<WayPoint> _wayPointQueue = null;
        private WayPoint _currWayPoint = null;

        //private HashSet<>
        private IWeightedActionExecutor _iWeightedActionExecutor = new WeightedActionExecutor();

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            _iWeightedActionExecutor?.Initialize(this);

            if (data != null &&
                !data.WayPoints.IsNullOrEmpty())
            {
                if(_wayPointQueue == null)
                    _wayPointQueue = new();

                _wayPointQueue.Clear();

                foreach (var wayPoint in data.WayPoints)
                {
                    _wayPointQueue?.Enqueue(wayPoint);
                }
            }
   
            return this;
        }
        
        public override void Begin()
        {
            Debug.Log("Begin()");

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetETeam(ETeam.Ally);
                ally?.Activate();
            }
            
            StartCombatAtWaypointAsync().Forget();
        }

        public override void ChainUpdate()
        {
            if (_currWayPoint != null)
            {
                if(_currWayPoint.AliveMonsterCount <= 0)
                {
                    _currWayPoint = null;
                    StartCombatAtWaypointAsync().Forget();

                    return;
                }
            }
                
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                _data?.AllyICombatantList[i]?.IActCtr?.ChainUpdate();
            }

            for (int i = 0; i < _currWayPoint?.EnemyICombatantList.Count; ++i)
            {
                _currWayPoint?.EnemyICombatantList[i].IActCtr?.ChainUpdate();
            }
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            var hpProgress = UICreator<HpProgress, HpProgress.Param>.Get?
                .Create();

            var targetPos = iCombatant.Transform.position;
            targetPos.y += iCombatant.Height;

            var param = new HpProgress.Param
            {
                TargetTm = iCombatant.Transform,
                Offset = new Vector2(0, iCombatant.Height + 0.5f),
            }.WithCombatant(iCombatant);

            hpProgress?.Activate(param);
        }

        private async UniTask StartCombatAtWaypointAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));

            if (!_wayPointQueue.TryDequeue(out _currWayPoint))
                return;

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];

                _iWeightedActionExecutor?.Execute(ally, this);
            }

            _iCameraManager.MoveToTarget(_currWayPoint.Position);

            for (int i = 0; i < _currWayPoint?.EnemyICombatantList?.Count; ++i)
            {
                var enemy = _currWayPoint?.EnemyICombatantList[i];
                if (enemy == null)
                    continue;

                enemy.SetETeam(ETeam.Enemy);
                enemy.Activate();

                CreateHpProgress(enemy);

                _iWeightedActionExecutor?.Execute(enemy, this);
            }
        }

        #region RealTime.IProvider

        WeightedActionParam IWeightedActionRequester.GetWeightedActionParam(ICombatant attacker, IWeightedAction iWeightedAction)
        {
            switch (iWeightedAction)
            {
                case WeightedAction<ApproachAttack.Param> approachAttack:
                    {
                        List<ICombatant> iCombatantList = null;
                        if (attacker.ETeam == ETeam.Ally)
                            iCombatantList = _currWayPoint?.EnemyICombatantList;
                        else if (attacker.ETeam == ETeam.Enemy)
                            iCombatantList = _data?.AllyICombatantList;

                        var param = new ApproachAttack.Param()
                            .WithAttacker(attacker)
                            .WithICombatantList(iCombatantList);

                        return param;
                    }
            }

            return null;
        }

        #endregion


        #region WeightedActionExecutor.IListener
        void WeightedActionExecutor.IListener.End(ICombatant iCombatant)
        {
            _iWeightedActionExecutor?.Execute(iCombatant, this);
        }
        #endregion
    }
}
