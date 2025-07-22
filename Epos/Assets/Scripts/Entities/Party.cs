using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using VContainer;

namespace Entities
{
    public interface IParty : IManager
    {
        Datas.ScriptableObjects.Party GetParty(int id);
    }
    
    public class Party : Manager, IParty
    {
        [SerializeField] private Datas.ScriptableObjects.Party[] parties = null;
        // private Dictionary<int, Datas.ScriptableObjects.Party> _partyDic = null;
        
        async UniTask IGeneric.InitializeAsync(IObjectResolver container)
        {
            await UniTask.CompletedTask;
        }
        
        public IGeneric Initialize()
        {
            // if (_partyDic == null)
            // {
            //     _partyDic = new();
            //     _partyDic.Clear();
            // }

            
            // MainManager.Get<ICharacterManager>()?.Create<Hero>(10001);
            // 저장된 데이터로 변경될 예정.
            // var formationInfo = new Info.Formation();
            // formationInfo.Index = 1;
            // formationInfo.CharacterIds[0, 0] = 10004;
            // formationInfo.CharacterIds[0, 1] = 10001;
            // formationInfo.CharacterIds[0, 2] = 10002;
            // formationInfo.CharacterIds[1, 0] = 0;
            // formationInfo.CharacterIds[1, 1] = 10003;
            // formationInfo.CharacterIds[1, 2] = 0;
            //
            // _formationList?.Add(formationInfo);
            
            return this;
        }
        
        #region IParty

        Datas.ScriptableObjects.Party IParty.GetParty(int id)
        {
            if (parties.IsNullOrEmpty())
                return null;

            for (int i = 0; i < parties.Length; ++i)
            {
                var party = parties[i];
                if(party == null)
                    continue;

                if (party.Id == id)
                    return party;
            }

            return null;
        }
        #endregion

        #region IManager

        // void IGeneric.ChainUpdate()
        // {
        //     
        // }
        //
        // void IGeneric.ChainLateUpdate()
        // {
        //     
        // }
        #endregion
    }
}

