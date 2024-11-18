using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Parts;

namespace UI.Panels
{
    public class BattleForces : Panel<BattleForces.Data>
    {
        public class Data : Base
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

        public override Panel<Data> Initialize(Data data)
        {
            base.Initialize(data);

            var leftForces = _data?.LeftForces?.CharacterList;
            if (leftForces != null)
            {
                for (int i = 0; i < leftForces.Count; ++i)
                {
                    if (leftForces[i] != null)
                    {
                        var gameObj = Instantiate(combatantGamaObj, leftForces[i].Position <= 3 ? leftFrontRootRectTm : leftRearRootRectTm);
                    }
                    else
                    {
                        var gameObj = Instantiate(emptyGamaObj, i <= 2 ? leftFrontRootRectTm : leftRearRootRectTm);
                    }
                }
            }
            
            
            return this;
        }
    }
}

