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
    }
}

