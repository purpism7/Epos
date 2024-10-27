using System.Collections;
using System.Collections.Generic;
using Info;
using UnityEngine;

namespace Creature.Action
{
    public interface ICastingController : IController<ICastingController, ICaster>
    {
        void Casting(Ability.Skill skill);
    }
    
    public class CastingController : Controller, ICastingController
    {
        private ICaster _iCaster = null;
        private List<Ability.Skill> _skillList = null;
        
        ICastingController IController<ICastingController, ICaster>.Initialize(ICaster iCaster)
        {
            _iCaster = iCaster;

            CreateSkillList();
            
            return this;
        }
        
        void IController<ICastingController, ICaster>.ChainUpdate()
        {
            
        }

        void ICastingController.Casting(Ability.Skill skill)
        {
            if (skill == null)
                return;
        }

        private void CreateSkillList()
        {
            if (_skillList == null)
            {
                _skillList = new();
                _skillList.Clear();
            }

            switch (_iCaster.Id)
            {
                case 90001:
                {
                    break;
                }
            }
        }
    }
}

