using UnityEngine;
using UnityEngine.UI;

namespace Dramon.Minesweeper
{
    /// <summary>남은 지뢰 수(지뢰 수 - 깃발 수)를 텍스트로 표시한다.</summary>
    public class MineCounterUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 보드를 알려주는 게임 상태 관리자")]
        [SerializeField] GameStateManager gameState;
        [Tooltip("숫자를 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 남은 지뢰 수가 들어가요. 예: Mines {0:000}")]
        [SerializeField] string format = "Mines {0:000}";

        int lastValue = int.MinValue;

        void Update()
        {
            if (gameState == null || label == null || gameState.CurrentBoard == null) return;
            int value = gameState.CurrentBoard.MinesLeft;
            if (value == lastValue) return;
            lastValue = value;
            label.text = string.Format(format, value);
        }
    }
}
