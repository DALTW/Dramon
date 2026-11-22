using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>현재 탐험 점수를 표시한다.</summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("점수를 가진 관리자")]
        [SerializeField] ExplorationScore score;
        [Tooltip("점수를 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 점수가 들어가요")]
        [SerializeField] string format = "Score {0}";

        int lastValue = int.MinValue;

        void Update()
        {
            if (score == null || label == null) return;
            int value = score.Score;
            if (value == lastValue) return;
            lastValue = value;
            label.text = string.Format(format, value);
        }
    }
}
