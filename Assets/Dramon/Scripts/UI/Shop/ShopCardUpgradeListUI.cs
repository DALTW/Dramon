using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 강화 탭: 지금 덱에 있는 카드 종류마다 "지금 → 강화 후" 줄을 만들고 [강화]를 처리한다 (카드 종류 단위 1단계).</summary>
    public class ShopCardUpgradeListUI : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] CardUpgrader upgrader;
        [SerializeField] CardStatCalculator calculator;
        [SerializeField] RectTransform listRoot;
        [SerializeField] ShopCardRow rowPrefab;
        [SerializeField] ShopToastUI toast;
        [Tooltip("강화할 카드가 없을 때 보일 문구 오브젝트")]
        [SerializeField] GameObject emptyLabel;

        [Header("표시")]
        [Tooltip("켜면 이미 강화한 카드도 목록에 보여요 (버튼은 꺼짐)")]
        [SerializeField] bool showUpgraded = true;

        [Header("문구")]
        [Tooltip("{0} = 지금 코스트, {1} = 강화 후 코스트")]
        [SerializeField] string costChangeFormat = "코스트 {0} → {1}";
        [Tooltip("{0} = 코스트")]
        [SerializeField] string costFormat = "코스트 {0}";
        [Tooltip("{0} = 지금 설명, {1} = 강화 후 설명")]
        [SerializeField] string changeFormat = "{0}  →  {1}";
        [Tooltip("{0} = 덱에 있는 장수")]
        [SerializeField] string countFormat = "덱에 {0}장";
        [SerializeField] string priceFormat = "{0} G";
        [SerializeField] string buttonText = "강화";
        [SerializeField] string upgradedText = "강화 완료";
        [SerializeField] string notEnoughMoneyText = "돈이 모자라요";
        [SerializeField] string notUpgradableText = "강화할 수 없어요";
        [Tooltip("{0} = 카드 이름, {1} = 낸 돈")]
        [SerializeField] string doneFormat = "{0}+ 강화 완료! 덱의 {0} 모두에 적용돼요 (-{1} G)";

        readonly List<ShopCardRow> rows = new List<ShopCardRow>();

        void OnEnable()
        {
            if (upgrader != null && upgrader.Wallet != null) upgrader.Wallet.MoneyChanged += HandleMoney;
            if (upgrader != null && upgrader.Deck != null) upgrader.Deck.Changed += Rebuild;
            if (upgrader != null && upgrader.State != null) upgrader.State.Changed += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (upgrader != null && upgrader.Wallet != null) upgrader.Wallet.MoneyChanged -= HandleMoney;
            if (upgrader != null && upgrader.Deck != null) upgrader.Deck.Changed -= Rebuild;
            if (upgrader != null && upgrader.State != null) upgrader.State.Changed -= Rebuild;
        }

        void HandleMoney(int _) => Rebuild();

        void Upgrade(CardData card)
        {
            if (upgrader == null) return;
            if (upgrader.TryUpgrade(card))
            {
                if (toast != null) toast.Show(string.Format(doneFormat, card.DisplayName, card.UpgradePrice), ShopFeedbackKind.Upgrade);
            }
            else if (toast != null) toast.Show(Reason(upgrader.Check(card)), ShopFeedbackKind.Fail);
        }

        string Reason(CardUpgrader.Block b) =>
            b == CardUpgrader.Block.NotEnoughMoney ? notEnoughMoneyText
            : b == CardUpgrader.Block.NotUpgradable ? notUpgradableText
            : b == CardUpgrader.Block.AlreadyUpgraded ? upgradedText : "";

        void Rebuild()
        {
            foreach (var r in rows) if (r != null) Destroy(r.gameObject);
            rows.Clear();
            if (upgrader == null || upgrader.Deck == null || rowPrefab == null || listRoot == null) return;
            foreach (var card in upgrader.Deck.DistinctCards())
            {
                if (card == null || !card.Upgradable) continue;
                bool done = upgrader.IsUpgraded(card);
                if (done && !showUpgraded) continue;
                string now = ShopCardBuyListUI.OneLine(calculator != null ? calculator.DescribeAs(card, false) : CardText.Format(card));
                string after = ShopCardBuyListUI.OneLine(calculator != null ? calculator.DescribeAs(card, true) : "");
                int c0 = card.CostFor(false), c1 = card.CostFor(true);
                string cost = c0 != c1 ? string.Format(costChangeFormat, c0, c1) : string.Format(costFormat, c0);
                var block = upgrader.Check(card);
                var row = Instantiate(rowPrefab, listRoot);
                row.Setup(card, card.DisplayName + (done ? "+" : ""), cost,
                    done ? after : string.Format(changeFormat, now, after),
                    string.Format(countFormat, upgrader.Deck.CountOf(card)),
                    done ? "" : string.Format(priceFormat, card.UpgradePrice), Reason(block),
                    done ? upgradedText : buttonText, block == CardUpgrader.Block.None, Upgrade);
                rows.Add(row);
            }
            if (emptyLabel != null) emptyLabel.SetActive(rows.Count == 0);
        }
    }
}
