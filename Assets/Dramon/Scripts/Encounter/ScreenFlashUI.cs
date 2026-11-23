using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>화면 전체를 하얗게 번쩍이게 한다 (등장 연출 ①).</summary>
    public class ScreenFlashUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("화면을 덮는 Image (Raycast Target은 꺼 두세요)")]
        [SerializeField] Image image;

        [Header("모양")]
        [Tooltip("번쩍임 색")]
        [SerializeField] Color color = Color.white;
        [Tooltip("가장 밝을 때의 불투명도 (0~1)")]
        [Range(0f, 1f)]
        [SerializeField] float peakAlpha = 0.9f;

        void Awake() => Clear();

        /// <summary>count번 번쩍인다. 한 번 = 밝아지기(halfDuration) + 어두워지기(halfDuration).</summary>
        public IEnumerator Flash(int count, float halfDuration)
        {
            if (image == null) yield break;
            image.enabled = true;
            for (int i = 0; i < count; i++)
            {
                yield return Fade(0f, peakAlpha, halfDuration);
                yield return Fade(peakAlpha, 0f, halfDuration);
            }
            Clear();
        }

        public void Clear()
        {
            if (image == null) return;
            image.color = new Color(color.r, color.g, color.b, 0f);
            image.enabled = false;
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                SetAlpha(Mathf.Lerp(from, to, t / duration));
                yield return null;
            }
            SetAlpha(to);
        }

        void SetAlpha(float a) => image.color = new Color(color.r, color.g, color.b, a);
    }
}
