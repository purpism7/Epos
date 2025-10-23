using UnityEngine;
using System.Collections.Generic;

using VContainer.Unity;

using Creature;
using Battle.Formation;

namespace GameSystem
{
    public interface IStrategyManager
    {
        void Initialize();
    }

    public class StrategyManager : IStrategyManager
    {
       
        #region IStrategyManager
        void IStrategyManager.Initialize()
        {

        }
        #endregion

    }
}

