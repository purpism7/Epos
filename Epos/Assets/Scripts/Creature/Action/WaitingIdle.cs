using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Creature.Action
{
    public class WaitingIdle : WeightedAction<WaitingIdle.Param>
    {
        public class Param : WeightedActionParam
        {

        }
        public override void Execute()
        {
            PlayAnimation(_param.AnimationKey, true);

            EndAsync().Forget();
        }

        private async UniTask EndAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            _endAction?.Invoke(_iActor);
        }
    }
}

