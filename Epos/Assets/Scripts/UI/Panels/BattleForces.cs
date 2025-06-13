using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Datas.ScriptableObjects;
using GameSystem;
using Parts;
using UI.Parts;
using UI.Slots;
using Common;

namespace UI.Panels
{
    public class BattleForces : UI.Panel<BattleForces.Data>
    {
        public class Data : Common.ComponentData
        {
            public Party AllyParty { get; private set; } = null;
            public Party EnemyParty { get; private set; } = null;

            public Data(Party allyParty, Party enemyParty)
            {
                AllyParty = allyParty;
                EnemyParty = enemyParty;
            }
        }
        
        [Header("Ally")]
        [SerializeField] private BattlePartyPart allyBattlePartyPart = null;
        
        [Header("Enemy")]
        [SerializeField] private BattlePartyPart enemyBattlePartyPart = null;
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);

            allyBattlePartyPart?.Initialize();
            enemyBattlePartyPart?.Initialize();
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            ActivateAllyBattleParty();
            ActivateEnemyBattleParty();
        }
        
        public override void Deactivate()
        {
            base.Deactivate();

            allyBattlePartyPart?.Deactivate();
            enemyBattlePartyPart?.Deactivate();
            // rightFrontRootRectTm.RemoveAllChild();
            // rightRearRootRectTm.RemoveAllChild();
            // leftFrontRootRectTm.RemoveAllChild();
            // leftRearRootRectTm.RemoveAllChild();
        }

        private void ActivateAllyBattleParty()
        {
            var battlePartyPartData = new BattlePartyPart.Data();
            battlePartyPartData
                .WithParty(_data?.AllyParty)
                .WithETeam(ETeam.Ally);
            allyBattlePartyPart?.Activate(battlePartyPartData);
        }

        private void ActivateEnemyBattleParty()
        {
            var battlePartyPartData = new BattlePartyPart.Data();
            battlePartyPartData
                .WithParty(_data?.EnemyParty)
                .WithETeam(ETeam.Enemy);
            enemyBattlePartyPart?.Activate(battlePartyPartData);
        }
    }
}

