using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dramon
{
    /// <summary>덱 탭: 지금 덱의 카드 종류와 장수를 보여 주고, [제거]로 카드 1장을 뺀다 (확인 창을 거쳐요).</summary>
    public class ShopDeckListUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] CardRemover remover;
        [SerializeField] CardStatCalculator calculator;
        [Tooltip("카드 강화가 바뀌면 이름(+)을 다시 써요")]
        [SerializeField] CardUpgradeState cardUpgrades;
        [SerializeField] RectTransform listRoot;
        [SerializeField] ShopCardRow rowPrefab;
        [SerializeField] ShopToastUI toast;
        [SerializeField] ShopConfirmDialog confirmDialog;
        [Tooltip("덱 장수와 제거 가격 안내")]
        [SerializeField] Text summaryLabel;

        [Header("설정")]
        [Tooltip("켜면 [제거] 버튼을 보여 줘요. 끄면 덱 보기만")]
        [SerializeField] bool allowRemove = true;

        [Header("문구")]
        [Tooltip("{0} = 덱 장수, {1} = 지금 제거 가격, {2} = 최소 장수")]
        [SerializeField] string summaryFormat = "덱 {0}장 · 카드 제거 {1} G (뺄 때마다 +25, 최소 {2}장)";
        [Tooltip("{0} = 코스트")]
        [SerializeField] string costFormat = "코스트 {0}";
        [Tooltip("{0} = 장수")]
        [SerializeField] string countFormat = "× {0}";
        [SerializeField] string priceFormat = "{0} G";
        [SerializeField] string buttonText = "제거";
        [SerializeField] string notEnoughMoneyText = "돈이 모자라요";
        [SerializeField] string deckTooSmallText = "덱이 너무 작아요";
        [Tooltip("{0} = 카드 이름, {1} = 가격")]
        [SerializeField] string confirmFormat = "{0} 1장을 덱에서 뺄까요?\n({1} G)";
        [Tooltip("{0} = 카드 이름, {1} = 낸 돈")]
        [SerializeField] string removedFormat = "{0} 1장을 덱에서 뺐어요 (-{1} G)";

        readonly List<ShopCardRow> rows = new List<ShopCardRow>();

        void OnEnable()
        {
            if (remover != null && remover.Wallet != null) remover.Wallet.MoneyChanged += HandleMoney;
            if (remover != null && remover.Deck != null) remover.Deck.Changed += Rebuild;
            if (cardUpgrades != null) cardUpgrades.Changed += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (remover != null && remover.Wallet != null) remover.Wallet.MoneyChanged -= HandleMoney;
            if (remover != null && remover.Deck != null) remover.Deck.Changed -= Rebuild;
            if (cardUpgrades != null) cardUpgrades.Changed -= Rebuild;
        }

        void HandleMoney(int _) => Rebuild();

        void AskRemove(CardData card)
        {
            if (remover == null) return;
            var block = remover.Check(card);
            if (block != CardRemover.Block.None)
            {
                if (toast != null) toast.Show(Reason(block), ShopFeedbackKind.Fail);
                return;
            }
            int price = remover.Price;
            if (confirmDialog != null) confirmDialog.Show(string.Format(confirmFormat, card.DisplayName, price), () => Remove(card));
            else Remove(card);
        }

        void Remove(CardData card)
        {
            int price = remover.Price;
            if (remover.TryRemove(card))
            {
                if (toast != null) toast.Show(string.Format(removedFormat, card.DisplayName, price), ShopFeedbackKind.Upgrade);
            }
            else if (toast != null) toast.Show(Reason(remover.Check(card)), ShopFeedbackKind.Fail);
        }

        string Reason(CardRemover.Block b) =>
            b == CardRemover.Block.NotEnoughMoney ? notEnoughMoneyText : b == CardRemover.Block.DeckTooSmall ? deckTooSmallText : "";

        void Rebuild()
        {
            foreach (var r in rows) if (r != null) Destroy(r.gameObject);
            rows.Clear();
            if (remover == null || remover.Deck == null) return;
            var deck = remover.Deck;
            if (summaryLabel != null) summaryLabel.text = string.Format(summaryFormat, deck.Count, remover.Price, remover.MinDeckSize);
            if (rowPrefab == null || listRoot == null) return;
            foreach (var card in deck.DistinctCards())
            {
                if (card == null) continue;
                bool up = calculator != null && calculator.IsUpgraded(card);
                var block = remover.Check(card);
                var row = Instantiate(rowPrefab, listRoot);
                row.Setup(card, card.DisplayName + (up ? "+" : ""),
                    string.Format(costFormat, calculator != null ? calculator.Cost(card) : card.Cost),
                    ShopCardBuyListUI.OneLine(calculator != null ? calculator.Describe(card) : CardText.Format(card)),
                    string.Format(countFormat, deck.CountOf(card)),
                    allowRemove ? string.Format(priceFormat, remover.Price) : "", allowRemove ? Reason(block) : "",
                    buttonText, block == CardRemover.Block.None, allowRemove ? AskRemove : (System.Action<CardData>)null);
                rows.Add(row);
            }
        }
    }
}
