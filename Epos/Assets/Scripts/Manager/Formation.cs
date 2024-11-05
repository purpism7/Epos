using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
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
            
            // 저장된 데이터로 변경될 예정.
            var formationInfo = new Info.Formation();
            formationInfo.CharacterIds[0, 1] = 10001;
            
            _formationList?.Add(formationInfo);
            
            Debug.Log(_formationList[0]?.CharacterIds[0,1]);
            
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

