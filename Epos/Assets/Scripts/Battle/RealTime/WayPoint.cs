using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using VContainer;

using Creature;
using Creator;

namespace Battle
{
    public class Waypoint : Common.Component
    {
        [Inject] protected IObjectResolver _iResolver = null;


        private Monster[] _monsters = null;
        private List<ICombatant> _enemyICombatantList = null;

        public List<ICombatant> EnemyICombatantList
        {
            get
            {
                var list = new List<ICombatant>();
                list.Clear();

                for (int i = 0; i < _enemyICombatantList?.Count; ++i)
                {
                    var iActor = _enemyICombatantList[i]?.IActor;
                    if (iActor == null)
                        continue;

                    if (iActor.IsAlive)
                        list?.Add(_enemyICombatantList[i]);
                }

                return list;
            }
        }
        public Vector3 Position => transform.position;
        public int AliveMonsterCount
        {
            get
            {
                int count = 0;
                for(int i = 0; i < _monsters?.Length; ++i)
                {
                    if (_monsters[i].IsAlive)
                        ++count;
                }

                return count;
            }
        }

        [Inject]
        private void Initialize(IObjectResolver iResolver)
        {
            Debug.Log("Initialize Waypoint");
        }

        public override void Initialize()
        {
            _monsters = GetComponentsInChildren<Monster>();

            _enemyICombatantList = new();
            _enemyICombatantList.Clear();

            var combatantCreator = _iResolver?.Resolve<CombatantCreator>();

            foreach (var monster in _monsters)
            {
                _iResolver?.Inject(monster);
                monster?.Initialize();

                _enemyICombatantList?.Add(combatantCreator?.Create(monster, monster.Skills));
            }
        }
    }
}

