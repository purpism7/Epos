using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    // public class Skill
    // {
    //     public virtual void ChainUpdate()
    //     {
    //         
    //     }
    // }
    
    public class Skill
    {
        // public class BaseData
        // {
        //     public int Id = 0;
        // }

        public Data.Skill SkillData { get; private set; } = null;

        // private T _data = null;
        
        public virtual void Initialize(int id)
        {
            SkillData = new Data.Skill(id, 5f);
        }

        public virtual void ChainUpdate()
        {
            
        }

        public virtual void Casting()
        {
            
        }
    }
}

