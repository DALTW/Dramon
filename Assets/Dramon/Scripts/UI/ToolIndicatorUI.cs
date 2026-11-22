using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>HUD 구석에 지금 들고 있는 도구 아이콘과 전환 키 안내를 보여준다.</summary>
    public class ToolIndicatorUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("현재 도구를 알려주는 컴포넌트")]
        [SerializeField] ToolSwitcher toolSwitcher;
        [Tooltip("도구별 아이콘 설정")]
        [SerializeField] ToolSettings toolSettings;
        [Tooltip("도구 아이콘을 표시할 Image")]
        [SerializeField] Image iconImage;
        [Tooltip("전환 키 안내를 표시할 Text")]
        [SerializeField] Text hintLabel;

        [Header("표시")]
        [Tooltip("안내 문구. {0} 자리에 전환 키 이름이 들어가요 (키를 바꾸면 자동으로 바뀜)")]
        [SerializeField] string hintFormat = "{0}: 도구 바꾸기";

        void OnEnable()
        {
            if (toolSwitcher != null) toolSwitcher.ToolChanged += Refresh;
        }

        void Start()
        {
            if (toolSwitcher != null) Refresh(toolSwitcher.CurrentTool);
        }

        void OnDisable()
        {
            if (toolSwitcher != null) toolSwitcher.ToolChanged -= Refresh;
        }

        void Refresh(PlayerTool tool)
        {
            if (iconImage != null && toolSettings != null)
            {
                iconImage.sprite = toolSettings.Get(tool).IndicatorIcon;
                iconImage.preserveAspect = true;
            }
            if (hintLabel != null && toolSwitcher != null)
                hintLabel.text = string.Format(hintFormat, toolSwitcher.GetBindingDisplayString());
        }
    }
}
