using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>레몬 표시 완료 안내 창: 걸린 시간과 받은 보상금. 확인 버튼은 GamePhaseController.BeginSwordHunt에 연결.</summary>
    public class MarkingResultUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("보상금 지급을 알려주는 계산기")]
        [SerializeField] MarkingRewardCalculator rewardCalculator;
        [Tooltip("창을 닫을 때를 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("완료 안내 창 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [Tooltip("걸린 시간을 표시할 Text")]
        [SerializeField] Text timeLabel;
        [Tooltip("받은 보상금을 표시할 Text")]
        [SerializeField] Text rewardLabel;

        [Header("표시")]
        [Tooltip("걸린 시간 문구. {0}=분, {1}=초")]
        [SerializeField] string timeFormat = "걸린 시간  {0:00}:{1:00}";
        [Tooltip("보상금 문구. {0}=보상금")]
        [SerializeField] string rewardFormat = "보상금  {0}";

        void OnEnable()
        {
            if (rewardCalculator != null) rewardCalculator.RewardGranted += Show;
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            if (panel != null) panel.SetActive(false);
        }

        void OnDisable()
        {
            if (rewardCalculator != null) rewardCalculator.RewardGranted -= Show;
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        void Show(float seconds, int reward)
        {
            int s = Mathf.FloorToInt(seconds);
            if (timeLabel != null) timeLabel.text = string.Format(timeFormat, s / 60, s % 60);
            if (rewardLabel != null) rewardLabel.text = string.Format(rewardFormat, reward);
            if (panel != null) panel.SetActive(true);
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.MarkingComplete && panel != null) panel.SetActive(false);
        }
    }
}
