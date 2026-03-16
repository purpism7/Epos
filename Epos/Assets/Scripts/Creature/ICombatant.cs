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
        IActor Actor { get; }
        IActController ActController { get; }

        TeamType TeamType { get; }
        //int PartyPosition { get; }

        void SetTeamType(TeamType teamType);
        void SetPosition(Vector3 position);
        UniTask HitAsync();
    }
}
