using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using VContainer;

using Creature;

namespace Battle
{
    public class Waypoint : Common.Component
    {
        [Inject] protected IObjectResolver _iResolver = null;

        private Monster[] _monsters = null;

        public List<ICombatant> EnemyICombatantList { get; private set; } = null;
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

            EnemyICombatantList = new();
            EnemyICombatantList.Clear();

            foreach (var monster in _monsters)
            {
                _iResolver?.Inject(monster);
                monster?.Initialize();
                
                EnemyICombatantList?.Add(monster.ICombatant);
            }
        }
    }
}

