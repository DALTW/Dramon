using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>거래 종류 (안내 문구·효과음 고르기용)</summary>
    public enum ShopFeedbackKind { Sell, Upgrade, Buy, Fail }

    /// <summary>상점 거래 알림: 짧은 안내 문구("불꽃 비늘 3개를 45 G에 팔았어요")와 효과음.</summary>
    public class ShopToastUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("안내 문구를 표시할 Text")]
        [SerializeField] Text label;
        [Tooltip("효과음을 낼 AudioSource (비워 두면 소리 없음)")]
        [SerializeField] AudioSource audioSource;

        [Header("표시")]
        [Tooltip("문구가 보이는 시간(초)")]
        [Min(0.1f)]
        [SerializeField] float showDuration = 2f;
        [Tooltip("사라지는 시간(초)")]
        [Min(0f)]
        [SerializeField] float fadeDuration = 0.4f;
        [Tooltip("실패 안내(돈이 모자라요 등) 글자 색")]
        [SerializeField] Color failColor = new Color(0.62f, 0.14f, 0.1f, 1f);

        [Header("효과음 (비워 두면 생략)")]
        [Tooltip("재료를 팔았을 때")]
        [SerializeField] AudioClip sellSound;
        [Tooltip("강화했을 때")]
        [SerializeField] AudioClip upgradeSound;
        [Tooltip("아이템을 샀을 때")]
        [SerializeField] AudioClip buySound;
        [Tooltip("살 수 없을 때")]
        [SerializeField] AudioClip failSound;

        Color normalColor;
        Coroutine routine;

        void Awake()
        {
            if (label != null)
            {
                normalColor = label.color;
                label.text = "";
            }
        }

        void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            if (label != null) label.text = "";
        }

        public void Show(string message, ShopFeedbackKind kind)
        {
            PlaySound(kind);
            if (label == null) return;
            if (routine != null) StopCoroutine(routine);
            label.text = message;
            var color = kind == ShopFeedbackKind.Fail ? failColor : normalColor;
            label.color = color;
            if (isActiveAndEnabled) routine = StartCoroutine(Hide(color));
        }

        IEnumerator Hide(Color color)
        {
            yield return new WaitForSecondsRealtime(showDuration);
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                label.color = new Color(color.r, color.g, color.b, color.a * (1f - t / fadeDuration));
                yield return null;
            }
            label.text = "";
            label.color = color;
            routine = null;
        }

        void PlaySound(ShopFeedbackKind kind)
        {
            if (audioSource == null) return;
            var clip = kind switch
            {
                ShopFeedbackKind.Sell => sellSound,
                ShopFeedbackKind.Upgrade => upgradeSound,
                ShopFeedbackKind.Buy => buySound,
                _ => failSound,
            };
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
