using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>플레이어 체력 창: 막대 + 숫자 + 방어 (Icon_Block). 결투 중에만 보인다</summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] PlayerBattleStats stats;
        [Tooltip("체력 창 (DuelScreen/Panel/PlayerHealthWindow)")]
        [SerializeField] GameObject window;
        [Tooltip("체력 막대 (가로 길이를 체력 비율만큼 줄여요)")]
        [SerializeField] Image fill;
        [SerializeField] Text healthLabel;
        [Tooltip("방어 표시 묶음 (방어가 0이면 숨겨요)")]
        [SerializeField] GameObject blockRoot;
        [SerializeField] Text blockLabel;

        [Header("표시")]
        [Tooltip("{0} = 현재, {1} = 최대")]
        [SerializeField] string format = "{0} / {1}";
        [SerializeField] Color highColor = new Color32(0x58, 0xC8, 0x48, 0xFF);
        [Range(0f, 1f)]
        [SerializeField] float midThreshold = 0.5f;
        [SerializeField] Color midColor = new Color32(0xF0, 0xC0, 0x30, 0xFF);
        [Range(0f, 1f)]
        [SerializeField] float lowThreshold = 0.25f;
        [SerializeField] Color lowColor = new Color32(0xE0, 0x40, 0x30, 0xFF);

        void OnEnable()
        {
            if (stats != null) stats.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (stats != null) stats.Changed -= Refresh;
        }

        public void Show()
        {
            if (window != null) window.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (window != null) window.SetActive(false);
        }

        void Refresh()
        {
            if (stats == null) return;
            float ratio = stats.MaxHealth > 0 ? (float)stats.Health / stats.MaxHealth : 0f;
            if (fill != null)
            {
                var rt = fill.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = new Vector2(ratio, 1f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                fill.color = ratio <= lowThreshold ? lowColor : ratio <= midThreshold ? midColor : highColor;
            }
            if (healthLabel != null) healthLabel.text = string.Format(format, stats.Health, stats.MaxHealth);
            if (blockRoot != null) blockRoot.SetActive(stats.Block > 0);
            if (blockLabel != null) blockLabel.text = stats.Block.ToString();
        }
    }
}
