using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>아이템 구매 목록의 한 줄: 아이콘·이름·설명·가격·보유 개수·재고·[구매]·못 사는 이유. (Prefabs/UI/Shop/ShopBuyRow)</summary>
    public class ShopBuyRow : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] Image icon;
        [SerializeField] Text nameLabel;
        [SerializeField] Text descriptionLabel;
        [SerializeField] Text priceLabel;
        [SerializeField] Text ownedLabel;
        [SerializeField] Text stockLabel;
        [SerializeField] Text reasonLabel;
        [SerializeField] Button buyButton;

        [Header("버튼 색")]
        [Tooltip("누를 수 있을 때 버튼 색")]
        [SerializeField] Color enabledButtonColor = Color.white;
        [Tooltip("누를 수 없을 때 버튼 색 (돈이 모자라거나 최대 단계·품절일 때)")]
        [SerializeField] Color disabledButtonColor = new Color(0.55f, 0.52f, 0.48f, 1f);

        public void Setup(ShopItemData item, string price, string owned, string stock, string reason, bool canBuy,
            Action<ShopItemData> buy)
        {
            if (icon != null)
            {
                icon.sprite = item.Icon;
                icon.enabled = item.Icon != null;
            }
            if (nameLabel != null) nameLabel.text = item.DisplayName;
            if (descriptionLabel != null) descriptionLabel.text = item.Description;
            if (priceLabel != null) priceLabel.text = price;
            if (ownedLabel != null) ownedLabel.text = owned;
            if (stockLabel != null) stockLabel.text = stock;
            if (reasonLabel != null) reasonLabel.text = reason;
            if (buyButton != null)
            {
                buyButton.interactable = canBuy;
                if (buyButton.image != null) buyButton.image.color = canBuy ? enabledButtonColor : disabledButtonColor;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => buy?.Invoke(item));
            }
        }
    }
}
