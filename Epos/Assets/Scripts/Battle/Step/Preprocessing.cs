using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameSystem;

namespace Battle.Step
{
    public partial class Preprocessing : BattleStep<Preprocessing.Data>
    {
        public class Data : BaseData
        {
            public System.Type Type { get; private set; } = null;

            public Data(System.Type type)
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
                    MainManager.Get<ICameraManager>()?.ZoomIn(data.CameraZoomInPos, 
                        () =>
                        {
                            data.CameraZoomInEndAction?.Invoke();

                            End();
                        });
                    
                    break;
                }
            }
        }
    }
}

