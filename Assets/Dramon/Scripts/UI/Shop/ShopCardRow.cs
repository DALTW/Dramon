using System;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>
    /// 카드 목록의 한 줄 (카드 구매·카드 강화·덱 보기 탭이 같이 씀): 카드 그림·이름·코스트·설명·정보·가격·버튼·못 하는 이유.
    /// (Prefabs/UI/Shop/ShopCardRow)
    /// </summary>
    public class ShopCardRow : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] Image cardImage;
        [SerializeField] Text nameLabel;
        [SerializeField] Text costLabel;
        [SerializeField] Text descriptionLabel;
        [SerializeField] Text infoLabel;
        [SerializeField] Text priceLabel;
        [SerializeField] Text reasonLabel;
        [SerializeField] Button actionButton;
        [Tooltip("버튼 글자")]
        [SerializeField] Text actionLabel;

        [Header("버튼 색")]
        [SerializeField] Color enabledButtonColor = Color.white;
        [SerializeField] Color disabledButtonColor = new Color(0.55f, 0.52f, 0.48f, 1f);

        public CardData Card { get; private set; }

        public void Setup(CardData card, string name, string cost, string description, string info, string price, string reason,
            string buttonText, bool canAct, Action<CardData> action)
        {
            Card = card;
            if (cardImage != null)
            {
                var sprite = card != null ? (card.ComposedImage != null ? card.ComposedImage : card.Art) : null;
                cardImage.sprite = sprite;
                cardImage.enabled = sprite != null;
            }
            if (nameLabel != null) nameLabel.text = name;
            if (costLabel != null) costLabel.text = cost;
            if (descriptionLabel != null) descriptionLabel.text = description;
            if (infoLabel != null) infoLabel.text = info;
            if (priceLabel != null) priceLabel.text = price;
            if (reasonLabel != null) reasonLabel.text = reason;
            if (actionButton != null)
            {
                actionButton.gameObject.SetActive(action != null);
                actionButton.interactable = canAct;
                if (actionButton.image != null) actionButton.image.color = canAct ? enabledButtonColor : disabledButtonColor;
                actionButton.onClick.RemoveAllListeners();
                if (action != null) actionButton.onClick.AddListener(() => action(card));
            }
            if (actionLabel != null && buttonText != null) actionLabel.text = buttonText;
        }
    }
}
