using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 시스템 커서를 숨기고, 들고 있는 도구 모양의 UI Image가 마우스를 따라다니게 한다.
    /// 창 밖·일시정지·결투 중에는 일반 커서로 돌아간다.
    /// </summary>
    public class ToolCursorView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 도구를 알려주는 컴포넌트")]
        [SerializeField] ToolSwitcher toolSwitcher;
        [Tooltip("도구별 커서 스프라이트·크기·기준점 설정")]
        [SerializeField] ToolSettings toolSettings;
        [Tooltip("결투 중인지 확인할 탐험 상태 관리자")]
        [SerializeField] ExplorationState explorationState;
        [Tooltip("마우스를 따라다닐 커서 Image (Raycast Target은 꺼 두세요)")]
        [SerializeField] Image cursorImage;

        [Header("전환 연출")]
        [Tooltip("켜면 도구를 바꿀 때 커서가 살짝 커졌다 돌아와요")]
        [SerializeField] bool pulseOnSwitch = true;
        [Tooltip("커졌다 돌아오는 데 걸리는 시간(초)")]
        [SerializeField, Min(0.01f)] float pulseDuration = 0.15f;
        [Tooltip("가장 커졌을 때 배율")]
        [SerializeField, Min(1f)] float pulseScale = 1.3f;

        [Header("일반 커서로 돌아가는 경우")]
        [Tooltip("켜면 결투 중에는 일반 커서를 보여줘요")]
        [SerializeField] bool systemCursorDuringDuel = true;
        [Tooltip("켜면 일시정지(Time.timeScale = 0) 중에는 일반 커서를 보여줘요")]
        [SerializeField] bool systemCursorWhenPaused = true;

        RectTransform cursorRect;
        RectTransform parentRect;
        Canvas canvas;
        bool customVisible;
        float pulseTimer = -1f;

        void Awake()
        {
            if (cursorImage != null)
            {
                cursorRect = cursorImage.rectTransform;
                parentRect = cursorRect.parent as RectTransform;
                canvas = cursorImage.canvas;
                cursorImage.raycastTarget = false;
                cursorImage.enabled = false; // 첫 프레임에 보일지 다시 판단
            }
        }

        void OnEnable()
        {
            if (toolSwitcher != null) toolSwitcher.ToolChanged += HandleToolChanged;
        }

        void Start()
        {
            if (toolSwitcher != null) Apply(toolSwitcher.CurrentTool);
        }

        void OnDisable()
        {
            if (toolSwitcher != null) toolSwitcher.ToolChanged -= HandleToolChanged;
            SetCustomVisible(false);
            Cursor.visible = true;
        }

        void HandleToolChanged(PlayerTool tool)
        {
            Apply(tool);
            if (pulseOnSwitch) pulseTimer = 0f;
        }

        void Apply(PlayerTool tool)
        {
            if (cursorImage == null || toolSettings == null) return;
            var look = toolSettings.Get(tool);
            cursorImage.sprite = look.cursorSprite;
            cursorImage.preserveAspect = true;
            cursorRect.sizeDelta = look.cursorSize;
            cursorRect.pivot = look.hotspot;
        }

        void Update()
        {
            if (cursorImage == null) return;
            var mouse = Mouse.current;
            Vector2 screenPos = mouse != null ? mouse.position.ReadValue() : Vector2.zero;

            bool show = mouse != null
                && Application.isFocused
                && screenPos.x >= 0 && screenPos.y >= 0 && screenPos.x <= Screen.width && screenPos.y <= Screen.height
                && !(systemCursorWhenPaused && Time.timeScale == 0f)
                && !(systemCursorDuringDuel && explorationState != null && explorationState.Phase == ExplorationPhase.Dueling);

            SetCustomVisible(show);
            if (!show) return;

            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out var local))
                cursorRect.localPosition = local;

            UpdatePulse();
        }

        void UpdatePulse()
        {
            if (pulseTimer < 0f) return;
            pulseTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(pulseTimer / pulseDuration);
            float scale = 1f + (pulseScale - 1f) * Mathf.Sin(t * Mathf.PI);
            cursorRect.localScale = new Vector3(scale, scale, 1f);
            if (t >= 1f)
            {
                pulseTimer = -1f;
                cursorRect.localScale = Vector3.one;
            }
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
