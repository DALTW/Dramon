using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>게임 오버 창: 이유 문구 표시. 다시 시작 버튼은 GamePhaseController.Restart에 연결.</summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 오버를 알려주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;
        [Tooltip("게임 오버 창 (자식 Panel, 평소에는 꺼져 있음)")]
        [SerializeField] GameObject panel;
        [Tooltip("게임 오버 이유를 표시할 Text")]
        [SerializeField] Text reasonLabel;

        [Header("이유별 문구")]
        [Tooltip("드래곤 굴을 직접 열었을 때")]
        [SerializeField] string openedNestText = "드래곤 굴을 열어 버렸어요!";
        [Tooltip("굴이 아닌 칸에 레몬을 놓았을 때")]
        [SerializeField] string wrongLemonText = "굴이 아닌 곳에 레몬을 놓았어요!";
        [Tooltip("검 사냥 결투에서 드래곤에게 졌을 때")]
        [SerializeField] string lostDuelText = "드래곤에게 지고 말았어요!";

        void OnEnable()
        {
            if (phaseController != null) phaseController.PhaseChanged += HandlePhaseChanged;
            if (panel != null) panel.SetActive(false);
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.PhaseChanged -= HandlePhaseChanged;
        }

        void HandlePhaseChanged(GamePhase phase)
        {
            bool show = phase == GamePhase.GameOver;
            if (show && reasonLabel != null)
                reasonLabel.text = phaseController.LastGameOverReason switch
                {
                    GameOverReason.WrongLemon => wrongLemonText,
                    GameOverReason.LostDuel => lostDuelText,
                    _ => openedNestText,
                };
            if (panel != null) panel.SetActive(show);
        }
    }
}
