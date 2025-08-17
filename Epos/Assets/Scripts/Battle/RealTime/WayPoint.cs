using Creature;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    public class Waypoint : Common.Component
    {
        private Monster[] _monsters = null;

        public List<ICombatant> EnemyICombatantList => _monsters.ToList<ICombatant>();
        public Vector3 Position => transform.position;
        public int AliveMonsterCount
        {
            get
            {
                int count = 0;
                for(int i = 0; i < _monsters.Length; ++i)
                {
                    if (_monsters[i].IsActivate)
                        ++count;
                }

                return count;
            }
        }

        public override void Initialize()
        {
            _monsters = GetComponentsInChildren<Monster>();
            foreach(var monster in _monsters)
            {
                monster?.Initialize();
            }
        }
    }
}

