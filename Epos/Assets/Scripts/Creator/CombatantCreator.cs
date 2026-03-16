using UnityEngine;

using VContainer;

using Datas.ScriptableObjects;
using Creature;
using Common;

namespace Creator
{
    public class CombatantCreator
    {
        [Inject] private IObjectResolver _iResolver = null;
        [Inject] private WeakTypeMap<IActor> _map = null;

        public ICombatant Create(IActor actor, Skill[] skills)
        {
            var combatant = new Combatant();
            _iResolver?.Inject(combatant);
            _map?.Set<ICombatant>(actor, combatant);

#if UNITY_EDITOR
            if (actor is Character character)
                character.Combatant = combatant;
#endif

            return combatant?.Initialize(actor, skills);
        }
    }
}

