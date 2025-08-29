using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;

namespace UI.Popups
{
    public class BattleStart : Popup<BattleStart.Param>
    {
        public class Param : Common.Param
        {
            
        }

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}