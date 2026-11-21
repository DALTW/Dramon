using UnityEngine;
using UnityEngine.UI;

namespace Dramon.Minesweeper
{
    /// <summary>경과 시간을 초 단위로 표시한다.</summary>
    public class TimerUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("경과 시간을 가진 게임 상태 관리자")]
        [SerializeField] GameStateManager gameState;
        [Tooltip("시간을 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 경과 초가 들어가요. 예: Time {0:000}")]
        [SerializeField] string format = "Time {0:000}";

        int lastValue = int.MinValue;

        void Update()
        {
            if (gameState == null || label == null) return;
            int value = Mathf.FloorToInt(gameState.ElapsedTime);
            if (value == lastValue) return;
            lastValue = value;
            label.text = string.Format(format, value);
        }
    }
}
