using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity;

using Creature.Action;

namespace Creature
{
    public interface ICaster : IActor
    {
        ISkillController ISkillCtr { get; }
    }
}
