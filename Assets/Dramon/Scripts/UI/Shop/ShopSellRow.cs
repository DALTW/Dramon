using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>재료 판매 목록의 한 줄: 아이콘·이름·희귀도·개수·1개 판매가, [1개 팔기] [모두 팔기]. (Prefabs/UI/Shop/ShopSellRow)</summary>
    public class ShopSellRow : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] Image icon;
        [SerializeField] Text nameLabel;
        [SerializeField] Text rarityLabel;
        [SerializeField] Text countLabel;
        [SerializeField] Text priceLabel;
        [SerializeField] Button sellOneButton;
        [SerializeField] Button sellAllButton;

        [Header("표시")]
        [Tooltip("개수 표시 형식. {0} = 개수")]
        [SerializeField] string countFormat = "x{0}";
        [Tooltip("판매가 표시 형식. {0} = 1개 판매가")]
        [SerializeField] string priceFormat = "{0} G";

        public void Setup(MaterialData material, int count, string rarityName, Color rarityColor,
            Action<MaterialData> sellOne, Action<MaterialData> sellAll)
        {
            if (icon != null)
            {
                icon.sprite = material.Icon;
                icon.enabled = material.Icon != null;
            }
            if (nameLabel != null)
            {
                nameLabel.text = material.DisplayName;
                nameLabel.color = rarityColor;
            }
            if (rarityLabel != null)
            {
                rarityLabel.text = rarityName;
                rarityLabel.color = rarityColor;
            }
            if (countLabel != null) countLabel.text = string.Format(countFormat, count);
            if (priceLabel != null) priceLabel.text = string.Format(priceFormat, material.SellPrice);
            Bind(sellOneButton, () => sellOne?.Invoke(material));
            Bind(sellAllButton, () => sellAll?.Invoke(material));
        }

        static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
