using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

using Spine.Unity;
using VContainer;
using Datas.ScriptableObjects;

using Common;
using Creature.Action;

namespace Creature
{
    public class Combatant : ICombatant
    {
        [Inject] private IObjectResolver _iResolver = null;

        private Color _originColor = Color.white;

        public IActor Actor { get; private set; } = null;
        public ISkillController ISkillCtr { get; private set; } = null;

        public Transform Transform { get { return Actor?.Transform; } }
        public IStat IStat { get { return Actor?.IStat; } }

        public TeamType TeamType { get; private set; } = TeamType.None;

        [Inject]
        private void InitializeInject(IObjectResolver iResolver)
        {
            //Debug.Log("InitializeInject = " + iResolver);
        }

        public ICombatant Initialize(IActor iActor, Skill[] skills)
        {
            Actor = iActor;

            var skeleton = Actor?.SkeletonAnimation?.Skeleton;
            if (skeleton != null)
                _originColor = skeleton.GetColor();

            InitializeSkillController(skills);

            return this;
        }

        void ICombatant.SetTeamType(TeamType teamType)
        {
            TeamType = teamType;
        }

        void ICombatant.SetPosition(Vector3 position)
        {
            Actor?.SetWorldPosition(position);
        }

        async UniTask ICombatant.HitAsync()
        {
            var skeleton = Actor?.SkeletonAnimation?.Skeleton;
            if (skeleton == null)
                return;

            Color color = new Color(1f, 0.4f, 0.4f, 1f);
            skeleton?.SetColor(color);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            skeleton?.SetColor(_originColor);
        }

        private void InitializeSkillController(Skill[] skills)
        {
            ISkillCtr = new SkillController(skills);
            _iResolver?.Inject(ISkillCtr);

            ISkillCtr?.Initialize(this);
        }
    }
}
