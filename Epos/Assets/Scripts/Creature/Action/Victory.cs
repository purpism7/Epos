using Cysharp.Threading.Tasks;
using Spine;
using System;
using UnityEngine;

namespace Creature.Action
{
    public class Victory : Act<Victory.Param>
    {
        public class Param : ActParam
        {

        }

        public override void Execute()
        {
            PlayAnimation("Victory", true);
        }
    }
}
