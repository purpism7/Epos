using UnityEngine;

using Characters;
using Actor;
using Commons;

namespace Managers
{
    public class CameraManager : Entity
    {
        private Camera _camera = null;
        private CharacterManager _characterManager = null;
        private Transform _targetTransform = null;
        
        private Vector3 _offset = new Vector3(0, 0, -10);
        
        public void Initialize(CharacterManager characterManager)
        {
            _characterManager = characterManager;
            
            _camera = Camera.main;
        }
        
        public override void Activate()
        {
            if (_characterManager == null)
                return;

            _characterManager.OnMainPlayerChanged -= OnMainPlayerChanged;
            _characterManager.OnMainPlayerChanged += OnMainPlayerChanged;
            
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
        
        public void ChainLateUpdate()
        {
            UpdateFollowToTarget();
        }

        private void UpdateFollowToTarget()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                return;
            }
            
            if (_targetTransform == null)
                return;
         
            Vector3 targetPosition = new Vector3(
                _targetTransform.position.x + _offset.x,
                _targetTransform.position.y + _offset.y,
                _offset.z
            );

            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                targetPosition,
                5f * Time.deltaTime
            );
        }

        private void OnMainPlayerChanged(PlayerCharacter playerCharacter)
        {
            if (playerCharacter != null)
            {
                _targetTransform =  playerCharacter.Transform;
            }
        }
    }
}

