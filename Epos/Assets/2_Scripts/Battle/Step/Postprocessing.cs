using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameSystem;

namespace Battle.Step
{
    public partial class Postprocessing : BattleStep<Postprocessing.Data>
    {
        public class Data : BaseData
        {
            public System.Type Type { get; private set; } = null;

            protected Data(System.Type type)
            {
                Type = type;
            }
        }

        public override void Begin()
        {
            if (_data == null)
                return;
            
            switch (_data)
            {
                case FieldData data:
                {
                    MainManager.Get<ICameraManager>()?.ZoomOut( 
                        () =>
                        {
                            data.CameraZoomOutEndAction?.Invoke();

                            End();
                        });
                    
                    break;
                }
            }
        }
    }
}


