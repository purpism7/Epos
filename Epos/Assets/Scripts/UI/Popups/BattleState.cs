using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;

namespace UI.Popups
{
    public class BattleState : Popup<BattleState.Param>
    {
        [SerializeField] 
        private RectTransform startRectTm = null;
        [SerializeField] 
        private RectTransform winRectTm = null;
        
        public class Param : Common.Param
        {
            
        }

        public override void Initialize(Param param)
        {
            base.Initialize(param);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            AllDeactive();
        }

        private void AllDeactive()
        {
            Extensions.SetActive(startRectTm, false);
            Extensions.SetActive(winRectTm, false);
        }
        
        public async UniTask StartAsync()
        {
            AllDeactive();
            Extensions.SetActive(startRectTm, true);

            await UniTask.Delay(TimeSpan.FromSeconds(2f));

            Deactivate();
        }
        
        public async UniTask WinAsync()
        {
            AllDeactive();
            Extensions.SetActive(winRectTm, true);

            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            Deactivate();
        }
    }
}
