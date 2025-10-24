using System.Collections.Generic;
using UnityEngine;

using Battle.Formation;
using Creature;
using GameSystem;

namespace Battle
{
    public interface IFormationController
    {
        void Initialize(List<ICombatant> allyICombatantList);
        void ChainUpdate();

        void ApplyFormation<T>() where T : BaseFormation, new();
        void MoveFormation(Vector3 targetPosition);

        ICombatant LeaderICombatant { get; }
    }

    public interface IFormationDataProvider
    {
        List<ICombatant> AllyICombatantList { get; }
    }

    public class FormationController : IFormationController, IFormationDataProvider
    {
        private IFormation _currentIFormation = null;
        public List<ICombatant> AllyICombatantList { get; private set; } = null;

        #region IFormationController
        void IFormationController.Initialize(List<ICombatant> allyICombatantList)
        {
            AllyICombatantList = allyICombatantList;

            ApplyFormation<Offensive>();
        }

        void IFormationController.ChainUpdate()
        {
            _currentIFormation?.ChainUpdate();
        }

        void IFormationController.ApplyFormation<T>()
        {
            ApplyFormation<T>();
        }

        void IFormationController.MoveFormation(Vector3 targetPosition)
        {
            _currentIFormation?.MoveFormation(targetPosition);
        }

        ICombatant IFormationController.LeaderICombatant
        {
            get
            {
                return _currentIFormation?.LeaderICombatant;
            }
        }
        #endregion

        private void ApplyFormation<T>() where T : BaseFormation, new()
        {
            _currentIFormation = new T();
            _currentIFormation?.Apply(this);
        }
    }

}
