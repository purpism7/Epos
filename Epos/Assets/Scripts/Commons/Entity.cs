using UnityEngine;

namespace Commons
{
    public interface IEntity
    {
        bool IsActive { get; }
        
        void Activate();
        void Deactivate();
        void ChainUpdate();
    }
    
    public class Entity : IEntity
    {
        public bool IsActive { get; protected set; } = false;
        
        public virtual void Activate()
        {
            IsActive = true;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
        }

        public virtual void ChainUpdate()
        {
            
        }
    }
    
    public class MonoEntity : MonoBehaviour, IEntity
    {
        // 인터페이스 명세 구현
        public bool IsActive { get; protected set; } = false;

        // 자주 쓰이는 유니티 트랜스폼 캐싱 (성능 최적화)
        protected Transform _cachedTransform = null;
        
        public virtual void Initialize()
        {
            _cachedTransform = transform;
        }

        public virtual void Activate()
        {
            IsActive = true;
            gameObject.SetActive(true); // 유니티 오브젝트 켜기
        }

        public virtual void Deactivate()
        {
            IsActive = false;
            gameObject.SetActive(false); // 유니티 오브젝트 끄기
        }

        // 중앙 매니저가 Update에서 호출해줄 체인 메서드
        public virtual void ChainUpdate()
        {
            // 이 컴포넌트는 유니티 Update()를 스스로 호출하지 않고, 
            // 상위 관리자가 호출해줄 때만 동작하므로 성능에 매우 유리합니다.
        }
    }
}

