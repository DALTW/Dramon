using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>능력 강화 목록의 한 줄: 아이콘·이름·설명·현재 단계·다음 단계 효과·가격·[강화]·못 하는 이유. (Prefabs/UI/Shop/ShopUpgradeRow)</summary>
    public class ShopUpgradeRow : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] Image icon;
        [SerializeField] Text nameLabel;
        [SerializeField] Text descriptionLabel;
        [SerializeField] Text levelLabel;
        [SerializeField] Text effectLabel;
        [SerializeField] Text priceLabel;
        [SerializeField] Text reasonLabel;
        [SerializeField] Button buyButton;

        [Header("버튼 색")]
        [Tooltip("누를 수 있을 때 버튼 색")]
        [SerializeField] Color enabledButtonColor = Color.white;
        [Tooltip("누를 수 없을 때 버튼 색 (돈이 모자라거나 최대 단계·품절일 때)")]
        [SerializeField] Color disabledButtonColor = new Color(0.55f, 0.52f, 0.48f, 1f);

        public void Setup(ShopUpgradeData upgrade, string level, string effect, string price, string reason, bool canBuy,
            Action<ShopUpgradeData> buy)
        {
            if (icon != null)
            {
                icon.sprite = upgrade.Icon;
                icon.enabled = upgrade.Icon != null;
            }
            if (nameLabel != null) nameLabel.text = upgrade.DisplayName;
            if (descriptionLabel != null) descriptionLabel.text = upgrade.Description;
            if (levelLabel != null) levelLabel.text = level;
            if (effectLabel != null) effectLabel.text = effect;
            if (priceLabel != null) priceLabel.text = price;
            if (reasonLabel != null) reasonLabel.text = reason;
            if (buyButton != null)
            {
                buyButton.interactable = canBuy;
                if (buyButton.image != null) buyButton.image.color = canBuy ? enabledButtonColor : disabledButtonColor;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => buy?.Invoke(upgrade));
            }
        }
    }
}
