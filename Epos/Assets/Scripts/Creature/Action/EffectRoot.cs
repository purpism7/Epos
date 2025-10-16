using UnityEngine;

using Common;

namespace Creature.Action
{
    public interface IEffectRoot
    {
        Transform Transform { get; }
        string EffectType  { get; }
    }

    public class EffectRoot : Common.Component, IEffectRoot
    {
        [SerializeField] private string effectType = string.Empty;

        Transform IEffectRoot.Transform => transform;
        string IEffectRoot.EffectType => effectType;

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}

