using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Battles.Simulation;

namespace Battles
{
    /// <summary>
    /// 판이 끝났을 때 화면 가운데에 띄우는 결과(승리 · 패배 · 무승부)와 다시 하기 안내. 표현 전용 — 판 흐름(SurvivalMode)이 켜고 끈다.
    /// 프리팹 없이 처음 한 번 화면 덮개(Canvas)와 글자를 만들어 두고 켜고 끄기만 한다. 크기 · 색은 ⚑ 임시값.
    /// </summary>
    public sealed class BattleResultView
    {
        // ── ⚑ 임시값 ──
        private const int CanvasSortingOrder = 100;
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private const float BandHeight = 360f;
        private const float TitleFontSize = 140f;
        private const float TitleOffsetY = 40f;
        private const float HintFontSize = 44f;
        private const float HintOffsetY = -110f;
        private const string HintText = "R  다시 하기";

        private static readonly Color BandColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color VictoryColor = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color DefeatColor = new Color(0.95f, 0.4f, 0.4f, 1f);
        private static readonly Color DrawColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static readonly Color HintColor = new Color(1f, 1f, 1f, 0.85f);

        private readonly GameObject _root = null;
        private readonly TextMeshProUGUI _title = null;

        /// <param name="font">한글 글리프가 있는 글꼴. null 이면 TMP 기본 글꼴(한글이 네모로 보일 수 있다).</param>
        public BattleResultView(TMP_FontAsset font)
        {
            if (font == null)
                Debug.LogWarning("[BattleResultView] 결과 글꼴이 비어 있어 TMP 기본 글꼴을 씁니다(한글이 깨질 수 있음). GameManager 프리팹의 Battle Result Font 를 채우세요.");

            _root = new GameObject("BattleResultView");

            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;

            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            var band = CreateBand(_root.transform);
            _title = CreateText("Title", band, font, TitleFontSize, TitleOffsetY);

            var hint = CreateText("Hint", band, font, HintFontSize, HintOffsetY);
            hint.text = HintText;
            hint.color = HintColor;

            _root.SetActive(false);
        }

        public void Show(BattleOutcome outcome)
        {
            // 장면과 함께 파괴됐으면(유니티 null) 아무것도 하지 않는다.
            if (_root == null || _title == null)
                return;

            _title.text = GetTitle(outcome);
            _title.color = GetTitleColor(outcome);
            _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null && _root.activeSelf)
                _root.SetActive(false);
        }

        private static string GetTitle(BattleOutcome outcome) => outcome switch
        {
            BattleOutcome.Victory => "승리",
            BattleOutcome.Defeat => "패배",
            BattleOutcome.Draw => "무승부",
            _ => string.Empty,
        };

        private static Color GetTitleColor(BattleOutcome outcome) => outcome switch
        {
            BattleOutcome.Victory => VictoryColor,
            BattleOutcome.Defeat => DefeatColor,
            _ => DrawColor,
        };

        // 화면 가로 전체 · 세로 가운데의 어두운 띠(글자가 전장 위에서도 읽히게).
        private static RectTransform CreateBand(Transform parent)
        {
            var bandObject = new GameObject("Band", typeof(RectTransform));
            var band = (RectTransform)bandObject.transform;
            band.SetParent(parent, false);
            band.anchorMin = new Vector2(0f, 0.5f);
            band.anchorMax = new Vector2(1f, 0.5f);
            band.pivot = new Vector2(0.5f, 0.5f);
            band.sizeDelta = new Vector2(0f, BandHeight);
            band.anchoredPosition = Vector2.zero;

            var image = bandObject.AddComponent<Image>();
            image.color = BandColor;
            image.raycastTarget = false;

            return band;
        }

        private static TextMeshProUGUI CreateText(string name, RectTransform parent, TMP_FontAsset font, float fontSize, float offsetY)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)textObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0f, fontSize * 1.4f);
            rect.anchoredPosition = new Vector2(0f, offsetY);

            var text = textObject.AddComponent<TextMeshProUGUI>();
            if (font != null)
                text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;

            return text;
        }
    }
}
