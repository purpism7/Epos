using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using Creature.Action;


namespace Creature
{
    public interface ICombatant : ICaster
    {
        IActor IActor { get; }
        
        ETeam ETeam { get; }
        //int PartyPosition { get; }

        void SetETeam(ETeam eTeam);
        void SetPosition(Vector3 position);
        UniTask HitAsync();
    }
}
