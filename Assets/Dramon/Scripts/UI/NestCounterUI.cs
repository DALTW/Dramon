using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>남은 둥지 수(둥지 수 - 경고 표식 수)를 표시한다.</summary>
    public class NestCounterUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("둥지 수를 가진 숲 보드")]
        [SerializeField] ForestBoard board;
        [Tooltip("숫자를 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 남은 둥지 수가 들어가요")]
        [SerializeField] string format = "Nests {0}";

        int lastValue = int.MinValue;

        void Update()
        {
            if (board == null || label == null) return;
            int value = board.NestsLeft;
            if (value == lastValue) return;
            lastValue = value;
            label.text = string.Format(format, value);
        }
    }
}
