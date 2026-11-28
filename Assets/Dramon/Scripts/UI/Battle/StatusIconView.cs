using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>상태 아이콘 한 칸: 그림 + 남은 수치 (Prefabs/UI/Battle/StatusIconView)</summary>
    public class StatusIconView : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] Text countLabel;

        public StatusType Type { get; private set; }

        public void Show(StatusType type, Sprite sprite, int count)
        {
            Type = type;
            if (icon != null) icon.sprite = sprite;
            if (countLabel != null) countLabel.text = count.ToString();
        }
    }
}
