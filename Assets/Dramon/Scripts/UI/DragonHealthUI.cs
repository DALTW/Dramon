using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>드래곤 체력 표시 창 (등장 연출 ⑤). 체력 막대만 보여 주고 이름·레벨 같은 다른 정보는 표시하지 않는다.</summary>
    public class DragonHealthUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("표시할 드래곤 체력")]
        [SerializeField] DragonHealth health;
        [Tooltip("체력 표시 창 (평소에는 꺼져 있음)")]
        [SerializeField] GameObject window;
        [Tooltip("체력 막대 Image (막대 틀 안에서 왼쪽 기준으로 줄어들어요)")]
        [SerializeField] Image fill;

        [Header("막대 색")]
        [Tooltip("체력이 넉넉할 때")]
        [SerializeField] Color highColor = new Color32(0x58, 0xC8, 0x48, 0xFF);
        [Tooltip("체력이 이 비율 이하이면 중간 색")]
        [Range(0f, 1f)]
        [SerializeField] float midThreshold = 0.5f;
        [SerializeField] Color midColor = new Color32(0xF0, 0xC0, 0x30, 0xFF);
        [Tooltip("체력이 이 비율 이하이면 위험 색")]
        [Range(0f, 1f)]
        [SerializeField] float lowThreshold = 0.2f;
        [SerializeField] Color lowColor = new Color32(0xE0, 0x40, 0x30, 0xFF);

        void Awake() => Hide();

        void OnEnable()
        {
            if (health != null) health.Changed += Refresh;
        }

        void OnDisable()
        {
            if (health != null) health.Changed -= Refresh;
        }

        public void Show()
        {
            if (window != null) window.SetActive(true);
            if (health != null) Refresh(health.Current, health.Max);
        }

        public void Hide()
        {
            if (window != null) window.SetActive(false);
        }

        void Refresh(int current, int max)
        {
            if (fill == null) return;
            float ratio = max > 0 ? (float)current / max : 0f;
            var rt = fill.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(ratio, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            fill.color = ratio <= lowThreshold ? lowColor : ratio <= midThreshold ? midColor : highColor;
        }
    }
}
