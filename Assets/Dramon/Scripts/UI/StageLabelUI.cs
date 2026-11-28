using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>지금 스테이지 번호 표시 ("1 스테이지")</summary>
    public class StageLabelUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] StageProgress stageProgress;
        [SerializeField] Text label;
        [Tooltip("결투 화면일 때 숨길 때 씀")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("켜고 끌 묶음 (비우면 이 오브젝트의 Image·Text만 숨겨요)")]
        [SerializeField] CanvasGroup group;

        [Header("숨기기")]
        [Tooltip("켜면 드래곤 등장·결투 중에는 숨겨요 (결투 화면 체력 창과 겹치지 않게)")]
        [SerializeField] bool hideDuringDuel = true;

        [Header("표시")]
        [Tooltip("{0} = 스테이지 번호, {1} = 모드 이름")]
        [SerializeField] string format = "{0} 스테이지";

        void OnEnable()
        {
            if (stageProgress != null) stageProgress.StageStarted += Refresh;
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            Refresh();
            if (phaseController != null) HandlePhaseChanged(phaseController.Phase);
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            bool hide = hideDuringDuel && (phase == GamePhase.Encounter || phase == GamePhase.Duel);
            if (group != null) group.alpha = hide ? 0f : 1f;
        }

        void OnDisable()
        {
            if (stageProgress != null) stageProgress.StageStarted -= Refresh;
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        void Refresh()
        {
            if (label == null || stageProgress == null) return;
            string modeName = stageProgress.Mode != null ? stageProgress.Mode.DisplayName : "";
            label.text = string.Format(format, stageProgress.Stage, modeName);
        }
    }
}
