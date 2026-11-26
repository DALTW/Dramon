using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>상점 창 위쪽의 현재 돈 표시. 돈이 바뀌면 숫자가 잠깐 커졌다 돌아온다.</summary>
    public class ShopMoneyUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("돈을 보관하는 지갑")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("돈을 표시할 Text")]
        [SerializeField] Text label;
        [Tooltip("동전 아이콘 (아직 없으면 비워 두세요 — 숨겨져요)")]
        [SerializeField] Image coinIcon;

        [Header("표시")]
        [Tooltip("표시 형식. {0} 자리에 돈이 들어가요")]
        [SerializeField] string format = "{0} G";

        [Header("강조")]
        [Tooltip("돈이 바뀔 때 숫자가 커지는 배율")]
        [Min(1f)]
        [SerializeField] float pulseScale = 1.3f;
        [Tooltip("커졌다 돌아오는 데 걸리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float pulseDuration = 0.25f;

        Vector3 baseScale = Vector3.one;
        bool baseRecorded;
        Coroutine pulse;

        void Awake() => RecordBase();

        void OnEnable()
        {
            if (wallet != null) wallet.MoneyChanged += HandleMoneyChanged;
            if (coinIcon != null) coinIcon.enabled = coinIcon.sprite != null;
            Refresh();
        }

        void OnDisable()
        {
            if (wallet != null) wallet.MoneyChanged -= HandleMoneyChanged;
            StopPulse();
        }

        void RecordBase()
        {
            if (baseRecorded || label == null) return;
            baseScale = label.rectTransform.localScale;
            baseRecorded = true;
        }

        void HandleMoneyChanged(int money)
        {
            Refresh();
            if (!isActiveAndEnabled || label == null || pulseDuration <= 0f) return;
            StopPulse();
            pulse = StartCoroutine(Pulse());
        }

        void Refresh()
        {
            if (label != null) label.text = string.Format(format, wallet != null ? wallet.Money : 0);
        }

        IEnumerator Pulse()
        {
            RecordBase();
            var rt = label.rectTransform;
            for (float t = 0f; t < pulseDuration; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.Sin(t / pulseDuration * Mathf.PI); // 0 → 1 → 0
                rt.localScale = baseScale * Mathf.Lerp(1f, pulseScale, k);
                yield return null;
            }
            rt.localScale = baseScale;
            pulse = null;
        }

        void StopPulse()
        {
            if (pulse != null) StopCoroutine(pulse);
            pulse = null;
            if (label != null && baseRecorded) label.rectTransform.localScale = baseScale;
        }
    }
}
