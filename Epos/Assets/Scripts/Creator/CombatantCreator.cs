using UnityEngine;

using Creature;
using VContainer;

namespace Creator
{
    public class CombatantCreator
    {
        [Inject] private IObjectResolver _iResolver = null;

        public ICombatant Create(IActor iActor)
        {
            Debug.Log("CombatantCreator = " + _iResolver + " / " + iActor.Id);
            var combatant = new Combatant();
            _iResolver?.Inject(combatant);

            return combatant?.Initialize(iActor, null);
        }
    }
}

