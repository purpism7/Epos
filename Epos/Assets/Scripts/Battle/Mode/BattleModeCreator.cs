using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

namespace Battle.Mode
{
    public class BattleModeCreator<T, V> where T : BattleMode, new() where V : BattleMode.BaseData
    {
        private V _data = null;
        
        public BattleModeCreator<T, V> SetData(V data)
        {
            _data = data;
            
            return this;
        }
        
        public BattleMode Create(IObjectResolver iResolver)
        {
            var battleMode = new T() as BattleMode<V>;
            iResolver?.Inject(battleMode);

            battleMode?.Initialize(_data);

            return battleMode;
        }
    }
}
