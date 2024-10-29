using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Skill = Ability.Skill;

namespace Creature.Action
{
    public interface ISkillController : IController<ISkillController, ICaster>
    {
        Skill PossibleActiveSkill { get; }
        void Casting(List<ICombatant> targetList);
    }
    
    public class SkillController : Controller, ISkillController
    {
        private ICaster _iCaster = null;
        private List<Ability.Skill> _skillList = null;
        
        ISkillController IController<ISkillController, ICaster>.Initialize(ICaster iCaster)
        {
            _iCaster = iCaster;

            CreateSkillList();
            
            return this;
        }
        
        void IController<ISkillController, ICaster>.ChainUpdate()
        {
            
        }

        Skill ISkillController.PossibleActiveSkill
        {
            get
            {
                return PossibleActiveSkill;
            }
        }
        
        void ISkillController.Casting(List<ICombatant> targetList)
        {
            // if (skill == null)
            //     return;
            var skill = PossibleActiveSkill;
            if (skill == null)
                return;

            var target = targetList?.FirstOrDefault();
            if (target == null)
                return;

            // var targetPos = target.Transform.position;
            // targetPos.x += skill.SkillData.Range;
            // targetPos.y -= 1f;
            // targetPos.z = 0;
            // Debug.Log("TargetPos = " + targetPos);
            // var speed = _iCaster.IStat.Get(Stat.EType.MoveSpeed);
            // var distance = Vector3.Distance(_iCaster.Transform.position, targetPos);
            // var duration = distance / speed;
            //
            // _iCaster?.IActCtr?.MoveToTarget(targetPos, () => { }, true);
            
            // _iCaster.Transform.DOMove(targetPos, duration)
            //     .OnComplete(() =>
            //     {
            //         
            //     });
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
                    var skill = new Skill();
                    skill.Initialize(1);
                    _skillList?.Add(skill);
                    
                    break;
                }
            }
        }

        private Skill PossibleActiveSkill
        {
            get
            {
                if (_skillList == null)
                    return null;

                foreach (var skill in _skillList)
                {
                    if(skill == null)
                        continue;
                    
                    return skill;
                }

                return null;
            }
        }
    }
}

