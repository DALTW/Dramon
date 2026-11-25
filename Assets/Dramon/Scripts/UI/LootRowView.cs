using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>전리품 창의 재료 한 줄 (아이콘 · 이름 · 개수). Prefabs/UI/LootRow.prefab에 붙어 있다.</summary>
    public class LootRowView : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("재료 아이콘")]
        [SerializeField] Image icon;
        [Tooltip("재료 이름")]
        [SerializeField] Text nameLabel;
        [Tooltip("개수")]
        [SerializeField] Text countLabel;

        [Header("표시")]
        [Tooltip("개수 표시 형식. {0} 자리에 개수가 들어가요")]
        [SerializeField] string countFormat = "x{0}";

        public void Set(MaterialData material, int count)
        {
            if (icon != null)
            {
                icon.sprite = material != null ? material.Icon : null;
                icon.enabled = icon.sprite != null;
            }
            if (nameLabel != null) nameLabel.text = material != null ? material.DisplayName : "";
            if (countLabel != null) countLabel.text = string.Format(countFormat, count);
        }
    }
}
