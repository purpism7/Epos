using UnityEngine;
using System.Collections.Generic;

using VContainer;

using Entities;
using Common;

namespace Creature.Action
{
    public interface ICreatureEffectController : IController<ICreatureEffectController, IActor>
    {
        void Activate(string effectName, Effect.Param effectParam, string effectType = "");
        void Deactivate(string effectType);
    }

    public class CreatureEffectController : Controller, ICreatureEffectController
    {
        [Inject] private IEffectManager _iEffectManager = null;

        private IActor _iActor = null;
        private IEffectRoot[] _iEffectRoots = null;

        private Dictionary<string, IEffect> _iEffectDic = null;

        #region IController
        ICreatureEffectController IController<ICreatureEffectController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iEffectDic = new();
            _iEffectDic.Clear();

            if(iActor.Transform)
                _iEffectRoots = iActor.Transform.GetComponentsInChildren<EffectRoot>();

            return this;
        }

        void IController<ICreatureEffectController, IActor>.ChainUpdate()
        {
            if (!IsActivate)
                return;

            if(_iEffectDic != null)
            {
                foreach (var iEffect in _iEffectDic.Values)
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

        private IEffectRoot GetIEffectRoot(string effectType)
        {
            if (_iEffectRoots.IsNullOrEmpty())
                return null;

            for(int i = 0; i < _iEffectRoots.Length; ++i)
            {
                var iEffectRoot = _iEffectRoots[i];
                if (iEffectRoot == null)
                    continue;

                if (string.IsNullOrEmpty(iEffectRoot.EffectType))
                    continue;

                if (iEffectRoot.EffectType == effectType)
                    return iEffectRoot;
            }

            return null;
        }

        #region ICreatureEffectController
        void ICreatureEffectController.Activate(string effectName, Effect.Param effectParam, string effectType)
        {
            //if (_iEffectDic == null)
            //    return;

            if (string.IsNullOrEmpty(effectName))
                return;

            // Transform rootTm = _iActor?.Transform;
            //IEffect iEffect = null;
            var effect = _iEffectManager?.GetEffect(effectName);
            if (effect == null)
                return;

            effect?.Activate();

            if (!string.IsNullOrEmpty(effectType))
            {
                var iEffectRoot = GetIEffectRoot(effectType);
                if (iEffectRoot == null) 
                    return;
                
                effectParam?.WithRootTm(iEffectRoot.Transform);
                
                //if (!_iEffectDic.TryGetValue(effectType, out iEffect))
                //{
                //    var effect = _iEffectManager?.GetEffect(effectName);
                //    _iEffectDic[effectType] = effect;

                //    iEffect = effect;
                //}
                //else
                //{
                //    iEffect.Activate();
                //}
            }
            else
            {
                //iEffect = _iEffectManager?.GetEffect(effectName);

                effectParam?.WithTargetPosition(_iActor?.Transform?.position);
            }

            effect?.ActivateAsync(effectParam);
        }

        void ICreatureEffectController.Deactivate(string effectType)
        {
            if(_iEffectDic != null &&
               _iEffectDic.TryGetValue(effectType, out var iEffect))
                iEffect?.Deactivate();
        }
        #endregion
    }
}

