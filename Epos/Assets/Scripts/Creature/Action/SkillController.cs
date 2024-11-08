using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Creature.Action
{
    public interface ISkillController : IController<ISkillController, ICaster>
    {
        Ability.Skill GetPossibleSkill(Type.ESkillCategory eSkillCategory);
        
        void Casting(List<ICombatant> targetList, Type.ESkillCategory eSkillCategory);
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

        Ability.Skill ISkillController.GetPossibleSkill(Type.ESkillCategory eSkillCategory)
        {
            return PossibleSkill(eSkillCategory);
        }
        
        void ISkillController.Casting(List<ICombatant> targetList, Type.ESkillCategory eSkillCategory)
        {
            // if (skill == null)
            //     return;
            var skill = PossibleSkill(eSkillCategory);
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
                case 10001:
                {
                    var skill = new Ability.Skill();
                    skill.Initialize(new Data.Skill(3, 3f));
                    skill.SetESkillCategory(Type.ESkillCategory.Passive);
                    skill.SetSameTeam(true);
                    skill.SetESkillTarget(Type.ESkillTarget.NearOne);
                    
                    _skillList?.Add(skill);
                    
                    break;
                }
                
                case 10003:
                {
                    // 소서리스
                    var skill = new Ability.Skill();
                    skill.Initialize(new Data.Skill(2, 0));
                    skill.SetESkillCategory(Type.ESkillCategory.Passive);
                    skill.SetSameTeam(true);
                    skill.SetESkillTarget(Type.ESkillTarget.All);
                    
                    _skillList?.Add(skill);
                    
                    break;
                }
                
                case 10004:
                {
                    // 스피어
                    var skill = new Ability.Skill();
                    skill.Initialize(new Data.Skill(4, 5f));
                    skill.SetESkillCategory(Type.ESkillCategory.Active);
                    skill.SetSameTeam(false);
                    skill.SetESkillTarget(Type.ESkillTarget.NearOne);
                    
                    _skillList?.Add(skill);
                    
                    break;
                }
                
                case 90001:
                {
                    var skill = new Ability.Skill();
                    skill.Initialize(new Data.Skill(1, 4f));
                    skill.SetESkillCategory(Type.ESkillCategory.Active);
                    skill.SetSameTeam(false);
                    skill.SetESkillTarget(Type.ESkillTarget.NearOne);
                    
                    _skillList?.Add(skill);
                    
                    break;
                }
            }
        }

        private Ability.Skill PossibleSkill(Type.ESkillCategory eSkillCategory)
        {
            if (_skillList == null)
                return null;

            foreach (var skill in _skillList)
            {
                if(skill == null)
                    continue;

                if (skill.ESkillCategory == eSkillCategory)
                    return skill;
            }

            return null;
        }
    }
}

