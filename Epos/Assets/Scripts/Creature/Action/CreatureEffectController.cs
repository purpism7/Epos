using UnityEngine;
using System.Collections.Generic;

using VContainer;

using Entities;
using Common;
using Cysharp.Threading.Tasks;

namespace Creature.Action
{
    public interface ICreatureEffectController : IController<ICreatureEffectController, IActor>
    {
        void Activate(string effectName, Effect.Param effectParam, string effectType = "");
        void Deactivate(string effectType);
    }

    public class CreatureEffectController : Controller, ICreatureEffectController
    {
        [Inject] private IEffectManager _effectManager = null;

        private IActor _actor = null;
        private IEffectRoot[] _effectRoots = null;

        private Dictionary<string, IEffect> _effects = null;

        #region IController
        ICreatureEffectController IController<ICreatureEffectController, IActor>.Initialize(IActor actor)
        {
            _actor = actor;

            _effects = new();
            _effects.Clear();

            if(actor.Transform)
                _effectRoots = actor.Transform.GetComponentsInChildren<EffectRoot>();

            return this;
        }

        void IController<ICreatureEffectController, IActor>.ChainUpdate()
        {
            if (!IsActivate)
                return;

            if(_effects != null)
            {
                foreach (var iEffect in _effects.Values)
                {
                    iEffect?.ChainUpdate();
                }
            }
        }

        void IController<ICreatureEffectController, IActor>.ChainFixedUpdate()
        {
            if (!IsActivate)
                return;
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
        #endregion

        private IEffectRoot GetEffectRoot(string effectType)
        {
            if (_effectRoots.IsNullOrEmpty())
                return null;

            for(int i = 0; i < _effectRoots.Length; ++i)
            {
                var effectRoot = _effectRoots[i];
                if (effectRoot == null)
                    continue;

                if (string.IsNullOrEmpty(effectRoot.EffectType))
                    continue;

                if (effectRoot.EffectType == effectType)
                    return effectRoot;
            }

            return null;
        }

        #region ICreatureEffectController
        void ICreatureEffectController.Activate(string effectName, Effect.Param effectParam, string effectType)
        {
            //if (_iEffectDic == null)
            //    return;

            ActivateAsync(effectName, effectParam, effectType).Forget();
        }

        private async UniTask ActivateAsync(string effectName, Effect.Param effectParam, string effectType = "")
        {
            if (string.IsNullOrEmpty(effectName))
                return;

            // Transform rootTm = _iActor?.Transform;
            IEffect effect = null;
  
            if (!string.IsNullOrEmpty(effectType))
            {
                var effectRoot = GetEffectRoot(effectType);
                if (effectRoot == null) 
                    return;
                
                effectParam?.WithRootTm(effectRoot.Transform)
                    .WithIsReturn(false);
                
                if (!_effects.TryGetValue(effectType, out effect))
                {
                    effect = await _effectManager.GetEffectAsync(effectName);
                    _effects[effectType] = effect;
                }
            }
            else
            {
                effect = await _effectManager.GetEffectAsync(effectName);
            }
            
            effect?.Activate();
            effect?.ActivateAsync(effectParam);
        }

        void ICreatureEffectController.Deactivate(string effectType)
        {
            if(_effects != null &&
               _effects.TryGetValue(effectType, out var iEffect))
                iEffect?.Deactivate();
        }
        #endregion
    }
}

