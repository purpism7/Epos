using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;

namespace Battle.Step
{
    public class BattleEnd : BattleStep<BattleEnd.Data>
    {
        public class Data : BaseData
        {
            public Action EndAction = null;
        }

        public override void Begin()
        {
            MainManager.Get<IFieldManager>()?.Activate();
            
            _data?.EndAction?.Invoke();

            End();
        }
    }
}

