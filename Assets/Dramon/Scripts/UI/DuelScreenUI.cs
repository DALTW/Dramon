using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 결투 화면 켜고 끄기, 임시 승패 버튼 보이기, 숲 화면으로 돌아갈 때 페이드 아웃.
    /// 게임 오버나 다시 시작이 되면 결투 화면을 바로 닫는다.
    /// </summary>
    public class DuelScreenUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 단계 관리자 (게임 오버·다시 시작 때 화면을 닫아요)")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("결투 화면 전체 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [Tooltip("페이드에 쓸 CanvasGroup (Panel에 붙어 있음)")]
        [SerializeField] CanvasGroup canvasGroup;
        [Tooltip("[임시] 결투 승리 / 결투 패배 버튼 묶음")]
        [SerializeField] GameObject buttons;

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            HideStage();
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        public void ShowStage()
        {
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (panel != null) panel.SetActive(true);
        }

        public void HideStage()
        {
            SetButtonsVisible(false);
            if (panel != null) panel.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        public void SetButtonsVisible(bool visible)
        {
            if (buttons != null) buttons.SetActive(visible);
        }

        /// <summary>결투 화면을 서서히 투명하게 한 뒤 닫는다.</summary>
        public IEnumerator FadeOut(float duration)
        {
            SetButtonsVisible(false);
            if (canvasGroup != null)
            {
                for (float t = 0f; t < duration; t += Time.deltaTime)
                {
                    canvasGroup.alpha = 1f - t / duration;
                    yield return null;
                }
            }
            HideStage();
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.GameOver || phase == GamePhase.LemonMarking) HideStage();
        }
    }
}
