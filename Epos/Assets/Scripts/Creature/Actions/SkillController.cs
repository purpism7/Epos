using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Common;
using Ability;

namespace Creature.Action
{
    public struct SkillStateInfo
    {
        public string Name;
        public string AnimationName;
        public Ability.Skill.EState State;
        public float CooldownLeft;
        public float CooldownTotal;
    }

    public interface ISkillController : IController<ISkillController, ICaster>
    {
        ISkill GetPossibleSkill(ESkillCategory eSkillCategory);
        IReadOnlyList<SkillStateInfo> GetSkillStates();
    }
    
    public class SkillController : Controller, ISkillController
    {
        #region Inspector
        
        #endregion
        
        private ICaster _iCaster = null;
        private Datas.ScriptableObjects.Skill[] _skillDatas = null;
        private List<Ability.ISkill> _iSkillList = null;
        private readonly List<SkillStateInfo> _skillStateInfos = new();

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
                skill.Initialize(skillData);

                _iSkillList?.Add(skill);
            }
        }

        ISkill ISkillController.GetPossibleSkill(ESkillCategory eSkillCategory)
        {
            return GetPossibleSkill(eSkillCategory);
        }

        IReadOnlyList<SkillStateInfo> ISkillController.GetSkillStates()
        {
            _skillStateInfos.Clear();
            if (_iSkillList == null)
                return _skillStateInfos;

            foreach (var iSkill in _iSkillList)
            {
                if (iSkill == null)
                    continue;

                var skillData = iSkill.SkillData;
                var skill = iSkill as Ability.Skill;
                _skillStateInfos.Add(new SkillStateInfo
                {
                    Name = skillData != null ? skillData.name : "—",
                    AnimationName = skillData != null ? skillData.AnimationName : string.Empty,
#if UNITY_EDITOR
                    State = skill != null ? skill.State : Ability.Skill.EState.None,
#endif
                    CooldownLeft = iSkill.CooldownLeft,
                    CooldownTotal = iSkill.CooldownTotal
                });
            }

            return _skillStateInfos;
        }

        private Ability.ISkill GetPossibleSkill(ESkillCategory eSkillCategory)
        {
            if (_iSkillList == null)
                return null;

            var stat = _iCaster?.IStat;
            if (stat == null)
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

                if (skillData.MP > stat.Get(Stat.EType.Mp))
                    continue;

                if (!iSkill.IsReady)
                    continue;
                
                return iSkill;
            }

            return null;
        }
    }
}
