using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>게임 오버 때 기록 창: 무한 모드면 도달 스테이지·최고 기록, 일반 모드면 쓰러진 스테이지. [모드 선택] 버튼.</summary>
    public class RunRecordUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] StageProgress stageProgress;
        [SerializeField] GamePhaseController phaseController;
        [SerializeField] ProgressSave save;
        [Tooltip("기록 창 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [SerializeField] Text label;

        [Header("문구")]
        [Tooltip("무한 모드. {0} = 도달 스테이지, {1} = 최고 스테이지")]
        [SerializeField] string endlessFormat = "무한 모드 {0} 스테이지 도달   (최고 {1} 스테이지)";
        [Tooltip("일반 모드. {0} = 스테이지")]
        [SerializeField] string normalFormat = "일반 모드 {0} 스테이지에서 쓰러졌어요";

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhase;
            if (panel != null) panel.SetActive(false);
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhase;
        }

        void HandlePhase(GamePhase phase)
        {
            bool show = phase == GamePhase.GameOver && stageProgress != null;
            if (show && label != null)
            {
                var mode = stageProgress.Mode;
                label.text = mode != null && mode.IsEndless
                    ? string.Format(endlessFormat, stageProgress.Stage, save != null ? save.EndlessBestStage : stageProgress.Stage)
                    : string.Format(normalFormat, stageProgress.Stage);
            }
            if (panel != null) panel.SetActive(show);
        }
    }
}
