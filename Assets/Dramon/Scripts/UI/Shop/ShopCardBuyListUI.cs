using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 구매 탭: 상점 목록의 카드마다 줄(ShopCardRow)을 만들고 [구매]를 처리한다.</summary>
    public class ShopCardBuyListUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] CardPurchaser purchaser;
        [Tooltip("파는 카드 목록")]
        [SerializeField] ShopCatalog catalog;
        [Tooltip("카드 설명의 최종 수치 (강화 반영)")]
        [SerializeField] CardStatCalculator calculator;
        [Tooltip("카드 강화가 바뀌면 설명을 다시 써요")]
        [SerializeField] CardUpgradeState cardUpgrades;
        [SerializeField] RectTransform listRoot;
        [SerializeField] ShopCardRow rowPrefab;
        [SerializeField] ShopToastUI toast;

        [Header("문구")]
        [Tooltip("{0} = 코스트")]
        [SerializeField] string costFormat = "코스트 {0}";
        [Tooltip("{0} = 덱에 있는 장수, {1} = 최대 보유 수")]
        [SerializeField] string ownedFormat = "덱에 {0}장 / 최대 {1}";
        [Tooltip("최대 보유 수가 없을 때. {0} = 덱에 있는 장수")]
        [SerializeField] string ownedNoLimitFormat = "덱에 {0}장";
        [SerializeField] string priceFormat = "{0} G";
        [SerializeField] string buttonText = "구매";
        [SerializeField] string notEnoughMoneyText = "돈이 모자라요";
        [SerializeField] string maxOwnedText = "더 가질 수 없어요";
        [Tooltip("{0} = 카드 이름, {1} = 낸 돈")]
        [SerializeField] string boughtFormat = "{0}을(를) 덱에 넣었어요! (-{1} G)";

        readonly List<ShopCardRow> rows = new List<ShopCardRow>();

        void OnEnable()
        {
            if (purchaser != null && purchaser.Wallet != null) purchaser.Wallet.MoneyChanged += HandleMoney;
            if (purchaser != null && purchaser.Deck != null) purchaser.Deck.Changed += Rebuild;
            if (cardUpgrades != null) cardUpgrades.Changed += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (purchaser != null && purchaser.Wallet != null) purchaser.Wallet.MoneyChanged -= HandleMoney;
            if (purchaser != null && purchaser.Deck != null) purchaser.Deck.Changed -= Rebuild;
            if (cardUpgrades != null) cardUpgrades.Changed -= Rebuild;
        }

        void HandleMoney(int _) => Rebuild();

        void Buy(CardData card)
        {
            if (purchaser == null) return;
            if (purchaser.TryBuy(card))
            {
                if (toast != null) toast.Show(string.Format(boughtFormat, card.DisplayName, card.ShopPrice), ShopFeedbackKind.Buy);
            }
            else if (toast != null) toast.Show(Reason(purchaser.Check(card)), ShopFeedbackKind.Fail);
        }

        string Reason(CardPurchaser.Block b) =>
            b == CardPurchaser.Block.MaxOwned ? maxOwnedText : b == CardPurchaser.Block.NotEnoughMoney ? notEnoughMoneyText : "";

        void Rebuild()
        {
            foreach (var r in rows) if (r != null) Destroy(r.gameObject);
            rows.Clear();
            if (catalog == null || rowPrefab == null || listRoot == null || purchaser == null) return;
            foreach (var card in catalog.Cards)
            {
                if (card == null) continue;
                bool up = calculator != null && calculator.IsUpgraded(card);
                int owned = purchaser.Deck != null ? purchaser.Deck.CountOf(card) : 0;
                string info = card.MaxOwned > 0 ? string.Format(ownedFormat, owned, card.MaxOwned) : string.Format(ownedNoLimitFormat, owned);
                var block = purchaser.Check(card);
                var row = Instantiate(rowPrefab, listRoot);
                row.Setup(card, card.DisplayName + (up ? "+" : ""),
                    string.Format(costFormat, calculator != null ? calculator.Cost(card) : card.Cost),
                    OneLine(calculator != null ? calculator.Describe(card) : CardText.Format(card)),
                    info, string.Format(priceFormat, card.ShopPrice), Reason(block), buttonText, block == CardPurchaser.Block.None, Buy);
                rows.Add(row);
            }
        }

        public static string OneLine(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("\n", " ");
    }
}
