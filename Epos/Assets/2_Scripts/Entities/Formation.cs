using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public interface IFormation : IManager
    {
        
    }
    
    public class Formation : IFormation
    {
        private List<Info.Formation> _formationList = null;

        public IGeneric Initialize()
        {
            if (_formationList == null)
            {
                _formationList = new();
                _formationList.Clear();
            }

            // MainManager.Get<ICharacterManager>()?.Create<Hero>(10001);
            // 저장된 데이터로 변경될 예정.
            var formationInfo = new Info.Formation();
            formationInfo.Index = 1;
            formationInfo.CharacterIds[0, 0] = 10004;
            formationInfo.CharacterIds[0, 1] = 10001;
            formationInfo.CharacterIds[0, 2] = 10002;
            formationInfo.CharacterIds[1, 0] = 0;
            formationInfo.CharacterIds[1, 1] = 10003;
            formationInfo.CharacterIds[1, 2] = 0;
            
            _formationList?.Add(formationInfo);
            
            return this;
        }

        #region IManager

        void IGeneric.ChainUpdate()
        {
            
        }
        
        void IGeneric.ChainLateUpdate()
        {
            
        }
        #endregion
    }
}

