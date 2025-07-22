using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameSystem;

namespace Battle.Step
{
    public partial class Preprocessing : BattleStep<Preprocessing.Param>
    {
        public class Param : BattleStep.BattleStepParam
        {
            public System.Type Type { get; private set; } = null;
            
            protected Param(System.Type type)
            {
                Type = type;
            }
        }
        
        public override void Begin()
        {
            if (_param == null)
                return;
            
            switch (_param)
            {
                case FieldParam param:
                {
                    MainManager.Get<ICameraManager>()?.ZoomIn(param.CameraZoomInPos, 
                        () =>
                        {
                            param.CameraZoomInEndAction?.Invoke();

                            End();
                        });
                    
                    break;
                }
            }
        }
    }
}

