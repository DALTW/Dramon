using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 비행 중 날갯짓에 맞춰 드래곤이 위아래로 오르내린다.
    /// 모션 전용 자식(Dragon/Motion)만 움직이고, DragonSide 위치·크기 같은 Scene 배치는 건드리지 않는다.
    /// 도트가 떨려 보이지 않게 이동값은 정수 픽셀로 끊는다.
    /// </summary>
    public class DragonHoverMotion : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("위아래로 움직일 모션 전용 오브젝트 (Dragon/Motion)")]
        [SerializeField] RectTransform target;

        [Header("흔들림")]
        [Tooltip("켜면 비행 중에 위아래로 흔들려요")]
        [SerializeField] bool useHover = true;

        float amplitude;
        float period = 1f;
        float time;
        bool playing;
        Vector2 restPosition;
        bool restRecorded;

        public bool IsPlaying => playing;

        void Awake() => RecordRest();

        void RecordRest()
        {
            if (restRecorded || target == null) return;
            restPosition = target.anchoredPosition;
            restRecorded = true;
        }

        /// <summary>흔들림 시작. amplitudePixels = 위아래 거리(픽셀), periodSeconds = 한 번 오르내리는 시간(초).</summary>
        public void Play(float amplitudePixels, float periodSeconds)
        {
            RecordRest();
            amplitude = Mathf.Max(0f, amplitudePixels);
            period = Mathf.Max(0.05f, periodSeconds);
            time = 0f;
            playing = useHover && amplitude > 0f;
            if (!playing) ResetPosition();
        }

        /// <summary>흔들림을 멈추고 Scene 배치 위치로 되돌린다.</summary>
        public void Stop()
        {
            playing = false;
            ResetPosition();
        }

        void Update()
        {
            if (!playing || target == null) return;
            time += Time.deltaTime;
            float y = Mathf.Round(amplitude * Mathf.Sin(time / period * Mathf.PI * 2f));
            target.anchoredPosition = restPosition + new Vector2(0f, y);
        }

        void ResetPosition()
        {
            RecordRest();
            if (target != null) target.anchoredPosition = restPosition;
        }
    }
}
