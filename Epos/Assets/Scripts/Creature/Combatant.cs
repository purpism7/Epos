using Common;
using Creature.Action;
using Cysharp.Threading.Tasks;
using Datas.ScriptableObjects;
using Spine.Unity;
using System;
using UnityEngine;
using VContainer;

namespace Creature
{
    public class Combatant : ICombatant
    {
        [Inject] private IObjectResolver _iResolver = null;

        private Color _originColor = Color.white;

        public IActor IActor { get; private set; } = null;
        public ISkillController ISkillCtr { get; private set; } = null;

        public Transform Transform { get { return IActor?.Transform; } }
        public IStat IStat { get { return IActor?.IStat; } }

        public ETeam ETeam { get; private set; } = ETeam.None;

        [Inject]
        private void InitializeInject(IObjectResolver iResolver)
        {
            //Debug.Log("InitializeInject = " + iResolver);
        }

        public ICombatant Initialize(IActor iActor, Skill[] skills)
        {
            IActor = iActor;

            var skeleton = IActor?.SkeletonAnimation?.Skeleton;
            if (skeleton != null)
                _originColor = skeleton.GetColor();

            InitializeSkillController(skills);

            return this;
        }

        void ICombatant.SetETeam(ETeam eTeam)
        {
            ETeam = eTeam;
        }

        void ICombatant.SetPosition(Vector3 position)
        {
            Transform.position = position;
        }

        async UniTask ICombatant.HitAsync()
        {
            var skeleton = IActor?.SkeletonAnimation?.Skeleton;
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
