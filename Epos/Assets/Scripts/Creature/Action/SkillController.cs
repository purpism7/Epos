using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Creature.Action
{
    public interface ISkillController : IController<ISkillController, ICaster>
    {
        Ability.Skill GetPossibleSkill(Type.ESkillCategory eSkillCategory);
        
        // void Casting(List<ICombatant> targetList, Type.ESkillCategory eSkillCategory);
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
        
        void IController<ISkillController, ICaster>.ChainFixedUpdate()
        {
            
        }

        Ability.Skill ISkillController.GetPossibleSkill(Type.ESkillCategory eSkillCategory)
        {
            return PossibleSkill(eSkillCategory);
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
                    skill.Initialize(new Datas.Skill(3, 3f, _iCaster.IStat.Get(Stat.EType.Attack)));
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
                    skill.Initialize(new Datas.Skill(2, 0, _iCaster.IStat.Get(Stat.EType.Attack)));
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
                    skill.Initialize(new Datas.Skill(4, 5f, _iCaster.IStat.Get(Stat.EType.Attack)));
                    skill.SetESkillCategory(Type.ESkillCategory.Active);
                    skill.SetSameTeam(false);
                    skill.SetESkillTarget(Type.ESkillTarget.NearOne);
                    
                    _skillList?.Add(skill);
                    
                    break;
                }
                
                case 90001:
                {
                    var skill = new Ability.Skill();
                    skill.Initialize(new Datas.Skill(1, 4f, _iCaster.IStat.Get(Stat.EType.Attack)));
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

                if (skill.ESkillCategory != eSkillCategory)
                    continue;
                
                if (eSkillCategory == Type.ESkillCategory.Active)
                {
                    if (_iCaster?.IStat?.Get(Stat.EType.ActivePoint) < 1)
                        continue;
                }

                if (eSkillCategory == Type.ESkillCategory.Passive)
                {
                    if (_iCaster?.IStat?.Get(Stat.EType.PassivePoint) < 1)
                        continue;
                }
                
                return skill;
            }

            return null;
        }
    }
}

