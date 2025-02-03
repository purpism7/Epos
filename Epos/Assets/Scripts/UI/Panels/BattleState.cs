using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Panels
{
    public class BattleState : Panel<BattleState.Data>
    {
        [SerializeField] 
        private RectTransform startRectTm = null;
        [SerializeField] 
        private RectTransform winRectTm = null;
        
        public class Data : BaseData
        {
            
        }

        public override Panel<Data> Initialize(Data data)
        {
            base.Initialize(data);

            return this;
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
