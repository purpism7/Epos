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
        private EffectRoot[] _effectRoots = null;

        private Dictionary<string, IEffect> _effects = null;

        #region IController
        ICreatureEffectController IController<ICreatureEffectController, IActor>.Initialize(IActor actor)
        {
            _actor = actor;

            _effects = new();
            _effects.Clear();

            var actorTransform = actor?.Transform;
            if(actorTransform)
                _effectRoots = actorTransform.GetComponentsInChildren<EffectRoot>();
            else
                _effectRoots = null;

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
                    if (IsValidEffect(iEffect))
                        iEffect.ChainUpdate();
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

        private EffectRoot GetEffectRoot(string effectType)
        {
            if (_effectRoots.IsNullOrEmpty())
                return null;

            for(int i = 0; i < _effectRoots.Length; ++i)
            {
                var effectRoot = _effectRoots[i];
                if (!effectRoot)
                {
                    _effectRoots[i] = null;
                    continue;
                }

                var iEffectRoot = (IEffectRoot)effectRoot;
                if (string.IsNullOrEmpty(iEffectRoot.EffectType))
                    continue;

                if (iEffectRoot.EffectType == effectType)
                    return effectRoot;
            }

            return null;
        }

        private static bool IsValidEffect(IEffect effect)
        {
            if (effect == null)
                return false;

            if (effect is UnityEngine.Object unityObject)
                return unityObject != null;

            return true;
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
            Transform rootTm = null;
  
            if (!string.IsNullOrEmpty(effectType))
            {
                var effectRoot = GetEffectRoot(effectType);
                if (!effectRoot)
                    return;

                rootTm = effectRoot.transform;
                if (!rootTm)
                    return;

                effectParam?.WithRootTm(rootTm)
                    .WithIsReturn(false);
                
                if (!_effects.TryGetValue(effectType, out effect) ||
                    !IsValidEffect(effect))
                {
                    effect = await _effectManager.GetEffectAsync(effectName);
                    _effects[effectType] = effect;
                }
            }
            else
            {
                effect = await _effectManager.GetEffectAsync(effectName);
            }

            if (!string.IsNullOrEmpty(effectType) && !rootTm)
                return;
            
            if (!IsValidEffect(effect))
                return;

            effect?.Activate();
            effect?.ActivateAsync(effectParam);
        }

        void ICreatureEffectController.Deactivate(string effectType)
        {
            if(_effects != null &&
               _effects.TryGetValue(effectType, out var iEffect) &&
               IsValidEffect(iEffect))
                iEffect.Deactivate();
        }
        #endregion
    }
}
