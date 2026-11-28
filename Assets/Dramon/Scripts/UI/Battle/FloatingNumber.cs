using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>떠오르는 숫자 한 개: 위로 떠오르며 흐려지다 사라진다 (Prefabs/UI/Battle/FloatingNumber)</summary>
    public class FloatingNumber : MonoBehaviour
    {
        [SerializeField] Text label;
        [Tooltip("떠오르는 높이(px)")]
        [SerializeField] float rise = 28f;
        [Tooltip("보이는 시간(초)")]
        [Min(0.1f)] [SerializeField] float duration = 0.9f;

        Vector2 start;
        float t;
        Color color;

        public void Play(Vector2 localPosition, string text, Color c)
        {
            start = localPosition;
            color = c;
            t = 0f;
            if (label != null) { label.text = text; label.color = c; }
            ((RectTransform)transform).anchoredPosition = start;
        }

        void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            ((RectTransform)transform).anchoredPosition = start + new Vector2(0f, rise * (1f - (1f - k) * (1f - k)));
            if (label != null) label.color = new Color(color.r, color.g, color.b, k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f);
            if (k >= 1f) Destroy(gameObject);
        }
    }
}
