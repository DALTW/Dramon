using UnityEngine;
using UnityEngine.UI;

namespace Dramon.Minesweeper
{
    /// <summary>승리·패배 시 결과 문구를 보여준다.</summary>
    public class GameResultUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("게임 상태를 알려주는 관리자")]
        [SerializeField] GameStateManager gameState;
        [Tooltip("결과를 표시할 Text")]
        [SerializeField] Text label;

        [Header("승리")]
        [Tooltip("모든 안전 칸을 열었을 때 문구")]
        [SerializeField] string winText = "CLEAR!";
        [SerializeField] Color winColor = new Color(0.45f, 0.9f, 0.45f);

        [Header("패배")]
        [Tooltip("지뢰를 밟았을 때 문구")]
        [SerializeField] string loseText = "BOOM!";
        [SerializeField] Color loseColor = new Color(1f, 0.4f, 0.35f);

        void OnEnable()
        {
            if (gameState != null) gameState.StateChanged += Show;
            Show(gameState != null ? gameState.State : GameState.Ready);
        }

        void OnDisable()
        {
            if (gameState != null) gameState.StateChanged -= Show;
        }

        void Show(GameState state)
        {
            if (label == null) return;
            switch (state)
            {
                case GameState.Won: label.text = winText; label.color = winColor; break;
                case GameState.Lost: label.text = loseText; label.color = loseColor; break;
                default: label.text = string.Empty; break;
            }
        }
    }
}
