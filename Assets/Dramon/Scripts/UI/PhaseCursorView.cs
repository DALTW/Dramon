using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 시스템 커서를 숨기고, 현재 단계에 맞는 커서 Image(레몬/검)가 마우스를 따라다니게 한다.
    /// 완료·게임 오버 창이 떠 있을 때, 창 밖, 일시정지 중에는 일반 커서로 돌아간다.
    /// </summary>
    public class PhaseCursorView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 단계를 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("단계별 커서 스프라이트·크기·기준점 설정")]
        [SerializeField] PhaseCursorSettings cursorSettings;
        [Tooltip("마우스를 따라다닐 커서 Image (Raycast Target은 꺼 두세요)")]
        [SerializeField] Image cursorImage;

        [Header("강조 (선택)")]
        [Tooltip("봉인된 굴 위에 있는지 알려주는 감지기. 연결하면 그 위에서 커서가 강조 스프라이트(highlightSprite)로 바뀌어요")]
        [SerializeField] SealedNestHoverDetector hoverDetector;

        [Header("일반 커서로 돌아가는 경우")]
        [Tooltip("켜면 일시정지(Time.timeScale = 0) 중에는 일반 커서를 보여줘요")]
        [SerializeField] bool systemCursorWhenPaused = true;

        RectTransform cursorRect;
        RectTransform parentRect;
        Canvas canvas;
        CursorAppearance current;
        bool customVisible;

        void Awake()
        {
            if (cursorImage == null) return;
            cursorRect = cursorImage.rectTransform;
            parentRect = cursorRect.parent as RectTransform;
            canvas = cursorImage.canvas;
            cursorImage.raycastTarget = false;
            cursorImage.enabled = false;
        }

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += Apply;
        }

        void Start()
        {
            if (phaseController != null) Apply(phaseController.Phase);
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= Apply;
            SetCustomVisible(false);
            Cursor.visible = true;
        }

        void Apply(GamePhase phase)
        {
            current = cursorSettings != null ? cursorSettings.Get(phase) : null;
            if (current == null || cursorImage == null) return;
            cursorImage.sprite = PickSprite();
            cursorImage.preserveAspect = true;
            cursorRect.sizeDelta = current.size;
            cursorRect.pivot = current.hotspot;
        }

        void Update()
        {
            if (cursorImage == null) return;
            var mouse = Mouse.current;
            Vector2 screenPos = mouse != null ? mouse.position.ReadValue() : Vector2.zero;

            bool show = current != null && current.sprite != null
                && mouse != null
                && Application.isFocused
                && screenPos.x >= 0 && screenPos.y >= 0 && screenPos.x <= Screen.width && screenPos.y <= Screen.height
                && !(systemCursorWhenPaused && Time.timeScale == 0f);

            SetCustomVisible(show);
            if (!show) return;

            var sprite = PickSprite();
            if (cursorImage.sprite != sprite) cursorImage.sprite = sprite;

            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out var local))
                cursorRect.localPosition = local;
        }

        /// <summary>강조 대상 위에 있고 강조 스프라이트가 있으면 강조 스프라이트, 아니면 기본 스프라이트</summary>
        Sprite PickSprite()
        {
            if (current == null) return null;
            bool highlight = hoverDetector != null && hoverDetector.IsHovering && current.highlightSprite != null;
            return highlight ? current.highlightSprite : current.sprite;
        }

        void SetCustomVisible(bool visible)
        {
            if (visible == customVisible) return;
            customVisible = visible;
            if (cursorImage != null) cursorImage.enabled = visible;
            Cursor.visible = !visible;
        }
    }
}
