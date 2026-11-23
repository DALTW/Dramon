using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 상점 버튼을 검 사냥 단계(커서가 검)에서만 누를 수 있게 한다.
    /// 버튼을 눌렀을 때 할 일(상점 열기)은 다음 단계에서 Button의 OnClick에 연결한다.
    /// </summary>
    public class ShopButtonUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 단계를 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("상점 버튼")]
        [SerializeField] Button button;

        [Header("비활성 표시")]
        [Tooltip("버튼을 흐리게 만들 CanvasGroup (버튼이 Sprite Swap이라 비활성 색이 따로 없어서 사용)")]
        [SerializeField] CanvasGroup canvasGroup;
        [Tooltip("비활성일 때 투명도 (0 = 안 보임, 1 = 그대로)")]
        [SerializeField, Range(0f, 1f)] float disabledAlpha = 0.45f;

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += Refresh;
        }

        void Start()
        {
            if (phaseController != null) Refresh(phaseController.Phase);
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= Refresh;
        }

        void Refresh(GamePhase phase)
        {
            bool active = phase == GamePhase.SwordHunt;
            if (button != null) button.interactable = active;
            if (canvasGroup != null) canvasGroup.alpha = active ? 1f : disabledAlpha;
        }
    }
}
