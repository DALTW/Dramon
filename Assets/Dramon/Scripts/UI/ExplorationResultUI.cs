using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>숲을 다 밝혔거나 게임 오버가 됐을 때 결과 문구를 보여준다.</summary>
    public class ExplorationResultUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("탐험 단계를 알려주는 상태 관리자")]
        [SerializeField] ExplorationState explorationState;
        [Tooltip("결과를 표시할 Text")]
        [SerializeField] Text label;

        [Header("클리어")]
        [Tooltip("둥지를 뺀 모든 칸을 밝혔을 때 문구")]
        [SerializeField] string clearedText = "Forest Cleared!";
        [SerializeField] Color clearedColor = new Color(1f, 0.9f, 0.35f);

        [Header("게임 오버")]
        [Tooltip("둥지를 열었을 때 문구 (1단계: 결투 없이 바로 게임 오버)")]
        [SerializeField] string gameOverText = "Game Over";
        [SerializeField] Color gameOverColor = new Color(1f, 0.4f, 0.35f);

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
            if (label == null) return;
            switch (phase)
            {
                case ExplorationPhase.Cleared: label.text = clearedText; label.color = clearedColor; break;
                case ExplorationPhase.GameOver: label.text = gameOverText; label.color = gameOverColor; break;
                default: label.text = string.Empty; break;
            }
        }
    }
}
