using UnityEngine;

namespace  UI.Parts
{
    public class HpProgress : PartWorld<HpProgress.Data>
    {
        public class Data : PartWorld<Data>.Data
        {
            
        }

        public override void Initialize(Data data)
        {
            base.Initialize(data);
        }

        private void LateUpdate()
        {
            //if(_data?.TargetTm)

            ChainLateUpdate();
        }
    }
}

