using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace UI.Popups
{
    public class BattleStart : Popup<BattleStart.Data>
    {
        public class Data : UI.ComponentData
        {
            
        }

        public override void Initialize(Data data)
        {
            base.Initialize(data);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}