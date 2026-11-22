using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// [임시] 카드 결투가 생기기 전까지, 결투 중에만 [결투 승리]/[결투 패배] 버튼 패널을 보여준다.
    /// 버튼의 OnClick은 DuelManager.WinDuel / LoseDuel에 연결돼 있다. 3단계에서 실제 결투 화면으로 교체.
    /// </summary>
    public class DuelPlaceholderUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("결투 중인지 알려주는 탐험 상태 관리자")]
        [SerializeField] ExplorationState explorationState;
        [Tooltip("결투 중에만 켜질 패널 (자식 Panel)")]
        [SerializeField] GameObject panel;

        void OnEnable()
        {
            if (explorationState != null) explorationState.PhaseChanged += Show;
            Show(explorationState != null ? explorationState.Phase : ExplorationPhase.Ready);
        }

        void OnDisable()
        {
            if (explorationState != null) explorationState.PhaseChanged -= Show;
        }

        void Show(ExplorationPhase phase)
        {
            if (panel != null) panel.SetActive(phase == ExplorationPhase.Dueling);
        }
    }
}
