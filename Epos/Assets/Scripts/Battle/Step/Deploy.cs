using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace Battle.Step
{
    public abstract class Deploy : BattleStep<Deploy.FieldData>
    {
        public class FieldData : BaseData
        {
            public Parts.Deploy Deploy = null;
        }
        
        public override void Begin()
        {
            DeloyAsync().Forget();
        }

        protected async UniTask DeloyAsync()
        {
            if(_data == null)
                return;
            
            _data.Deploy?.Activate();

            await UniTask.Yield();
            
            End();
        }    
    }
}

