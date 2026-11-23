using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>화면을 검게 덮는 와이프 전환 (등장 연출 ②)과, 덮은 화면을 걷어 내기.</summary>
    public class ScreenWipeTransition : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("화면을 덮는 검은 Image (왼쪽에서 오른쪽으로 늘어나요)")]
        [SerializeField] Image image;

        [Header("모양")]
        [Tooltip("덮는 색")]
        [SerializeField] Color color = Color.black;

        void Awake() => Clear();

        /// <summary>왼쪽에서 오른쪽으로 화면을 덮는다.</summary>
        public IEnumerator Cover(float duration)
        {
            if (image == null) yield break;
            image.enabled = true;
            image.color = color;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                SetWidth(Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            SetWidth(1f);
        }

        /// <summary>덮인 화면을 서서히 걷어 낸다 (불투명 → 투명).</summary>
        public IEnumerator Uncover(float duration)
        {
            if (image == null) yield break;
            SetWidth(1f);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                image.color = new Color(color.r, color.g, color.b, 1f - t / duration);
                yield return null;
            }
            Clear();
        }

        public void Clear()
        {
            if (image == null) return;
            SetWidth(0f);
            image.color = color;
            image.enabled = false;
        }

        /// <summary>화면 왼쪽부터 width(0~1) 비율만큼 덮는다.</summary>
        void SetWidth(float width)
        {
            var rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(width, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
