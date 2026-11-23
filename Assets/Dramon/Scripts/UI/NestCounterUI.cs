using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>남은 굴 수를 표시한다. (레몬 표시 단계: 전체 굴 - 놓은 레몬)</summary>
    public class NestCounterUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("레몬 표시 단계: 남은 굴 수(전체 굴 - 놓은 레몬)를 가진 집계기. 연결돼 있으면 이 값을 표시해요")]
        [SerializeField] NestMarkCounter markCounter;
        [Tooltip("(옛 방식) 경고 표식 기준 남은 둥지 수를 가진 숲 보드. Mark Counter가 비어 있을 때만 써요")]
        [SerializeField] ForestBoard board;
        [Tooltip("숫자를 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 남은 둥지 수가 들어가요")]
        [SerializeField] string format = "Nests {0}";

        int lastValue = int.MinValue;

        void Update()
        {
            if (label == null || (markCounter == null && board == null)) return;
            int value = markCounter != null ? markCounter.NestsLeft : board.NestsLeft;
            if (value == lastValue) return;
            lastValue = value;
            label.text = string.Format(format, value);
        }
    }
}
