using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class Skill
    {
        public virtual void ChainUpdate()
        {
            
        }
    }
    
    public abstract class Skill<T> : Skill
    {
        // public class BaseData
        // {
        //     public int Id = 0;
        // }
        
        protected Data.Skill _skillData = null;

        // private T _data = null;
        
        public virtual void Initialize(int id)
        {
            // _data = data;
            _skillData = new Data.Skill(id, 3f);

        }
    }
}

