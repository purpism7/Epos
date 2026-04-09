using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

using VContainer;
using Spine;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;

using Common;
using Creator;
using GameSystem;
using Item;
using UI;
using RotateMode = DG.Tweening.RotateMode;
using Sequence = DG.Tweening.Sequence;

namespace Creature.Action
{
    public class Die : Act<Die.Param>
    {
        public class Param : ActParam
        {
            
        }

        [Inject] private ItemFactory _itemFactory = null;
        [Inject] private ObjectPooler _objectPooler = null;
        [Inject] private UIFactory _uiFactory = null;
        [Inject] private UIManager _uiManager = null;

        public override void Execute()
        {
            PlayAnimation(_param.AnimationKey, false);

            // Eff_MonsterDead_01 이펙트 실행
            if (_actor is Monster)
            {
                var effectParam = new Effect.Param()
                    .WithRootTm(_actor?.Transform)
                    .WithReturnParent(true);
                
                _actor?.EffectController?.Activate("Eff_MonsterDead_01", effectParam);
            }
            
            DeactivateAsync().Forget();
            CreateDropItemAsync().Forget();
        }

        private async UniTask DeactivateAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            _actor?.Deactivate();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);
            
            _actor?.Deactivate();
        }

        private async UniTask CreateDropItemAsync()
        {
            if (_actor == null)
                return;
            
            var dropItem = _itemFactory?.Create<DropItem>(null);
            if (dropItem == null)
                return;

            var position = _actor.Transform.position;
            
            var dropItemParam = new DropItem.Param(_actor.SortingOrder, position);
            dropItem.Activate(dropItemParam);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            dropItem.Deactivate();
            _objectPooler?.Return(dropItem, true);

            var uiCreator = _uiFactory?.Create<CollectItem, CollectItem.Param>(_iResolver).SetRoot(_uiManager.CollectRootRectTr);
            var collectItem = uiCreator?.Create();
            if (collectItem != null)
            {
                Collect(collectItem.GetComponent<RectTransform>(), position);
            }
        }
        
        private void Collect(RectTransform rectTr, Vector3 worldStartPos)
        {
            if (!rectTr) return;
    
            // 1. 초기화 및 위치 고정
            rectTr.SetParent(_uiManager.CollectRootRectTr);
            rectTr.sizeDelta = new Vector2(80f, 80f); // 아이템 크기
            rectTr.localScale = Vector3.zero; // 처음엔 안보이다가 팝업되게

            // 월드(몬스터) -> UI 로컬 좌표 변환
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldStartPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _uiManager.CollectRootRectTr, 
                screenPos, 
                _uiManager.UICamera, 
                out Vector2 localPoint
            );
    
            Vector3 startPos = new Vector3(localPoint.x, localPoint.y, 0);
            rectTr.localPosition = startPos;

            // 2. 목적지 설정 (화면 상단 중앙)
            Vector3 endPos = new Vector3(0, 550f, 0); 

            // 3. 포물선의 정점(Mid Point) 계산
            // 시작과 끝의 중간 지점에서 옆으로 살짝 밀어주면 예쁜 곡선이 됩니다.
            Vector3 midPos = (startPos + endPos) / 2f;
            midPos.x += UnityEngine.Random.Range(-250f, 250f); // 좌우로 무작위 휘어짐
            // midPos.y += 150f; // 위로 살짝 들려야 포물선 느낌이 남

            // 4. DOTween 연출
            rectTr.DOKill();
            Sequence seq = DOTween.Sequence()
                .SetUpdate(true);

            // [연출 A] 뿅 나타나기
            seq.Append(rectTr.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
    
            // [연출 B] 포물선 이동 (0.8초~1.2초 정도가 적당히 느리고 좋습니다)
            seq.Append(rectTr.DOLocalPath(new[] { startPos, midPos, endPos }, 1.0f, PathType.CatmullRom)
                .SetEase(Ease.InQuad)); // 뒤로 갈수록 빨라지는 자석 효과

            // [연출 C] 회전 및 사라지기
            seq.Join(rectTr.DORotate(new Vector3(0, 0, 180f), 1.0f, RotateMode.FastBeyond360));
            seq.Join(rectTr.DOScale(0.5f, 1.0f));

            seq.OnUpdate(() => {
                // Z축 쓰레기 값 방지
                var p = rectTr.localPosition;
                p.z = 0;
                rectTr.localPosition = p;
            });

            seq.OnComplete(() => rectTr.gameObject.SetActive(false));
        }
        
    }
}
