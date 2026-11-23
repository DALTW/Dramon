using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>레몬 표시 경과 시간을 "분:초"로 표시한다.</summary>
    public class MarkingTimerUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("경과 시간을 가진 타이머")]
        [SerializeField] MarkingTimer timer;
        [Tooltip("시간을 표시할 Text")]
        [SerializeField] Text label;

        [Header("표시")]
        [Tooltip("표시 형식. {0}=분, {1}=초")]
        [SerializeField] string format = "{0:00}:{1:00}";

        int lastSeconds = -1;

        void Update()
        {
            if (timer == null || label == null) return;
            int seconds = Mathf.FloorToInt(timer.Elapsed);
            if (seconds == lastSeconds) return;
            lastSeconds = seconds;
            label.text = string.Format(format, seconds / 60, seconds % 60);
        }
    }
}
