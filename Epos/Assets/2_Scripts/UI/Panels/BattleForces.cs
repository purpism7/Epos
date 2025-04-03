using System.Collections;
using System.Collections.Generic;
using Creator;
using UnityEngine;

using GameSystem;
using Parts;
using UI.Parts;

namespace UI.Panels
{
    public class BattleForces : UI.Panel<BattleForces.Data>
    {
        public class Data : UI.Component.Data
        {
            public Forces LeftForces = null;
            public Forces RightForces = null;
        }

        [SerializeField] 
        private GameObject combatantGamaObj = null;
        [SerializeField] 
        private GameObject emptyGamaObj = null;
        
        [SerializeField] 
        private RectTransform rightFrontRootRectTm = null;
        [SerializeField] 
        private RectTransform rightRearRootRectTm = null;
        [SerializeField] 
        private RectTransform leftFrontRootRectTm = null;
        [SerializeField] 
        private RectTransform leftRearRootRectTm = null;

        public override void Initialize(Data data)
        {
            base.Initialize(data);

            var leftForces = _data?.LeftForces?.CharacterList;
            if (leftForces != null)
            {
                // BattleCombatant battleCombatant = null;
                // GameObject battleCombatantGameObject = null;
                for (int i = 0; i < leftForces.Count; ++i)
                {
                    if (leftForces[i] != null)
                    {
                         // UICreator<BattleCombatant, BattleCombatant.Data>()
                             
                         UICreator<BattleCombatant, BattleCombatant.Data>.Get?
                             .SetData(new BattleCombatant.Data()
                             {
                                 ICombatant = leftForces[i],
                             })
                             .SetRoot(leftForces[i].Position <= 3 ? leftFrontRootRectTm : leftRearRootRectTm)
                             .Create()?.Activate();
                    }
                    // battleCombatantGameObject = Instantiate(combatantGamaObj, leftForces[i].Position <= 3 ? leftFrontRootRectTm : leftRearRootRectTm);
                    else
                    {
                        Instantiate(emptyGamaObj, i <= 2 ? leftFrontRootRectTm : leftRearRootRectTm);
                    }
                }
            }
            
            var rightForces = _data?.RightForces?.CharacterList;
            if (rightForces != null)
            {
                for (int i = 0; i < rightForces.Count; ++i)
                {
                    if (rightForces[i] != null)
                    {
                        UICreator<BattleCombatant, BattleCombatant.Data>.Get?
                            .SetData(new BattleCombatant.Data()
                            {
                                ICombatant = rightForces[i],
                            })
                            .SetRoot(rightForces[i].Position <= 3 ? rightFrontRootRectTm : rightRearRootRectTm)
                            .Create()?.Activate();
                    }
                    else
                        Instantiate(emptyGamaObj, i <= 2 ? rightFrontRootRectTm : rightRearRootRectTm);
                }
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            rightFrontRootRectTm.RemoveAllChild();
            rightRearRootRectTm.RemoveAllChild();
            leftFrontRootRectTm.RemoveAllChild();
            leftRearRootRectTm.RemoveAllChild();
        }
    }
}

