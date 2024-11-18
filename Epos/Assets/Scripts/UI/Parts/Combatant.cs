using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace UI.Parts
{
    public class BattleCombatant : Part
    {
        [SerializeField] 
        private int position = 0;
        [SerializeField] 
        private string level = null;
        [SerializeField] 
        private Image combatantImg = null;
        [SerializeField] 
        private Image hpProgress = null;
    }
}

