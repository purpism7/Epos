using Ability;
using Common;
using Datas.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Creature.Action
{
    public interface ISkillController : IController<ISkillController, ICaster>
    {
        ISkill GetPossibleSkill(ESkillCategory eSkillCategory);
        
        // void Casting(List<ICombatant> targetList, Type.ESkillCategory eSkillCategory);
    }
    
    public class SkillController : Controller, ISkillController
    {
        #region Inspector
        
        #endregion
        
        private ICaster _iCaster = null;
        private Datas.ScriptableObjects.Skill[] _skillDatas = null;
        private List<Ability.ISkill> _iSkillList = null;

        public SkillController(Datas.ScriptableObjects.Skill[] skillDatas)
        {
            _skillDatas = skillDatas;
        }

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

        private void CreateSkillList()
        {
            if(_skillDatas == null || _skillDatas.Length <= 0)
                return;

            _iSkillList = new();
            _iSkillList.Clear();

            for(int i = 0; i < _skillDatas.Length; ++i)
            {
                var skillData = _skillDatas[i];
                if(skillData == null)
                    continue;   

                var skill = new Ability.Skill();
                skill?.Initialize(skillData);

                _iSkillList?.Add(skill);
            }
        }

        ISkill ISkillController.GetPossibleSkill(ESkillCategory eSkillCategory)
        {
            return GetPossibleSkill(eSkillCategory);
        }

        private Ability.ISkill GetPossibleSkill(ESkillCategory eSkillCategory)
        {
            if (_iSkillList == null)
                return null;

            foreach (var iSkill in _iSkillList)
            {
                if(iSkill == null)
                    continue;

                var skillData = iSkill.SkillData;
                if (skillData == null)
                    continue;

                if (skillData.ESkillCategory != eSkillCategory)
                    continue;


                if (!iSkill.IsReady)
                    continue;
                
                
                // if (eSkillCategory == ESkillCategory.Active)
                // {
                //     if (_iCaster?.IStat?.Get(Stat.EType.ActivePoint) < 1)
                //         continue;
                // }
                //
                // if (eSkillCategory == ESkillCategory.Passive)
                // {
                //     if (_iCaster?.IStat?.Get(Stat.EType.PassivePoint) < 1)
                //         continue;
                // }
                
                return iSkill;
            }

            return null;
        }
    }
}

