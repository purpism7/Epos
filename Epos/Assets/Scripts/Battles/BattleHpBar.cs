using UnityEngine;

using Battles.Simulation;
using Characters;

namespace Battles
{
    /// <summary>
    /// 머리 위 HP 바(표현 전용). 시뮬 유닛을 읽기만 하고 바꾸지 않는다.
    /// - 캐릭터 아래에 바탕 · 채움 스프라이트 두 장을 만들어 붙인다(캐릭터와 함께 움직이고 함께 사라진다).
    /// - 채움 길이 = HP / 최대 HP. HP 가 바뀐 프레임에만 고친다.
    /// - 채움 색은 편으로 가른다(아군 파랑 · 적 빨강 — 팀컬러 원리). 전투 불능이 되면 숨기고, 다시 하기로 일어서면 다시 켠다.
    /// - 높이는 처음 그려진 그림의 위 끝에서 한 번만 정한다(애니메이션마다 오르내리지 않게). 그 전에는 숨겨 둔다.
    /// </summary>
    public sealed class BattleHpBar
    {
        // ── ⚑ 임시값(카메라 정사영 크기 20 에서 읽히는 정도) ──
        private const float Width = 2f;
        private const float Height = 0.3f;
        private const float Border = 0.06f;
        private const float GapAboveHead = 0.3f;
        // 그림의 크기를 못 읽을 때 쓰는 머리 높이(캐릭터 원점에서 위로).
        private const float FallbackHeadHeight = 3f;
        // 그림이 아직 그려지지 않아 크기가 0 이면 이 프레임 수만큼 기다린 뒤 FallbackHeadHeight 를 쓴다.
        private const int MaxPlaceAttempts = 30;
        // 캐릭터의 그리기 순서(-y × 100 · 화면 안이면 ±수천)보다 늘 위. sortingOrder 상한은 32767.
        private const int BackgroundSortingOrder = 32000;
        private const int FillSortingOrder = 32001;
        private const float MinScale = 0.0001f;

        private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color AllyFillColor = new Color(0.25f, 0.55f, 1f, 1f);
        private static readonly Color EnemyFillColor = new Color(0.95f, 0.25f, 0.25f, 1f);

        // 모든 바가 같이 쓰는 1 × 1 단위 흰 사각형(왼쪽 가운데가 기준점 → 가로로 줄이면 오른쪽부터 준다).
        private static Sprite _unitSprite = null;

        private readonly Transform _characterTransform = null;
        private readonly Renderer _bodyRenderer = null;
        private readonly GameObject _rootObject = null;
        private readonly Transform _fill = null;

        private bool _isPlaced = false;
        private int _placeAttempts = 0;
        private int _shownHp = -1;

        public BattleHpBar(Character character, BattleTeam team)
        {
            _characterTransform = character != null ? character.Transform : null;
            if (_characterTransform == null)
                return;

            var skeletonAnimation = character.SkeletonAnimation;
            _bodyRenderer = skeletonAnimation != null ? skeletonAnimation.GetComponent<MeshRenderer>() : null;
            int sortingLayerId = _bodyRenderer != null ? _bodyRenderer.sortingLayerID : 0;

            _rootObject = new GameObject("HpBar");
            _rootObject.SetActive(false);   // 높이를 정하기 전에는 숨긴다.

            var root = _rootObject.transform;
            root.SetParent(_characterTransform, false);
            root.localScale = InverseScale(_characterTransform.lossyScale);   // 캐릭터 크기와 상관없이 같은 크기로.

            CreateQuad("Background", root, new Vector2(-Width * 0.5f - Border, 0f),
                new Vector2(Width + Border * 2f, Height + Border * 2f), BackgroundColor, sortingLayerId, BackgroundSortingOrder);
            _fill = CreateQuad("Fill", root, new Vector2(-Width * 0.5f, 0f),
                new Vector2(Width, Height), team == BattleTeam.Ally ? AllyFillColor : EnemyFillColor, sortingLayerId, FillSortingOrder);
        }

        /// <summary>프레임마다 부른다(위치를 옮긴 뒤). HP 가 바뀌었을 때만 채움을 고친다.</summary>
        public void Present(BattleUnit unit)
        {
            // 캐릭터와 함께 파괴됐으면(유니티 null) 아무것도 하지 않는다.
            if (unit == null || _rootObject == null || _fill == null)
                return;

            if (unit.IsDown)
            {
                if (_rootObject.activeSelf)
                    _rootObject.SetActive(false);

                return;
            }

            if (!_isPlaced && !TryPlaceAboveHead())
                return;

            // 처음 놓였을 때 · 다시 하기로 일어섰을 때 켠다.
            if (!_rootObject.activeSelf)
                _rootObject.SetActive(true);

            if (unit.Hp == _shownHp)
                return;

            _shownHp = unit.Hp;
            float ratio = Mathf.Clamp01((float)unit.Hp / Mathf.Max(1, unit.MaxHp));
            _fill.localScale = new Vector3(Width * ratio, Height, 1f);
        }

        // 그림의 위 끝 + 틈에 바를 놓는다. 그림이 아직 그려지지 않았으면 false(다음 프레임에 다시).
        private bool TryPlaceAboveHead()
        {
            float headHeight = FallbackHeadHeight;

            if (_bodyRenderer != null)
            {
                var bounds = _bodyRenderer.bounds;
                if (bounds.size.y > MinScale)
                    headHeight = bounds.max.y - _characterTransform.position.y;
                else if (++_placeAttempts < MaxPlaceAttempts)
                    return false;
            }

            float parentScaleY = Mathf.Max(MinScale, Mathf.Abs(_characterTransform.lossyScale.y));
            _rootObject.transform.localPosition = new Vector3(0f, (headHeight + GapAboveHead) / parentScaleY, 0f);
            _isPlaced = true;

            return true;
        }

        private static Transform CreateQuad(string name, Transform parent, Vector2 localPosition, Vector2 size,
            Color color, int sortingLayerId, int sortingOrder)
        {
            var quadObject = new GameObject(name);
            var quad = quadObject.transform;
            quad.SetParent(parent, false);
            quad.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            quad.localScale = new Vector3(size.x, size.y, 1f);

            var spriteRenderer = quadObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetUnitSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingLayerID = sortingLayerId;
            spriteRenderer.sortingOrder = sortingOrder;

            return quad;
        }

        private static Sprite GetUnitSprite()
        {
            // 유니티 null 비교라 플레이를 다시 켜서 사라진 경우에도 새로 만든다.
            if (_unitSprite == null)
            {
                var texture = Texture2D.whiteTexture;
                _unitSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0f, 0.5f), texture.width);
                _unitSprite.name = "BattleHpBarUnit";
            }

            return _unitSprite;
        }

        private static Vector3 InverseScale(Vector3 scale)
        {
            return new Vector3(1f / Mathf.Max(MinScale, Mathf.Abs(scale.x)), 1f / Mathf.Max(MinScale, Mathf.Abs(scale.y)), 1f);
        }
    }
}
