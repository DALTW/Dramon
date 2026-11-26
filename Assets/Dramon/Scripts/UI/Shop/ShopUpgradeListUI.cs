using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 능력 강화 탭: 상점 목록의 강화마다 줄(ShopUpgradeRow Prefab)을 만들고, [강화]를 처리한다.
    /// 돈이 모자라거나 최대 단계면 버튼을 끄고 이유를 보여 준다. (강화 효과는 카드 결투 단계에서 사용)
    /// </summary>
    public class ShopUpgradeListUI : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("강화 구매 처리")]
        [SerializeField] UpgradePurchaser purchaser;
        [Tooltip("돈이 바뀌면 버튼 상태를 다시 계산해요")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("상점에 보일 강화 목록")]
        [SerializeField] ShopCatalog catalog;
        [Tooltip("줄이 들어갈 곳 (스크롤 Content, Vertical Layout Group)")]
        [SerializeField] RectTransform listRoot;
        [Tooltip("강화 한 줄 Prefab")]
        [SerializeField] ShopUpgradeRow rowPrefab;
        [Tooltip("거래 안내 문구")]
        [SerializeField] ShopToastUI toast;

        [Header("표시")]
        [Tooltip("{0} = 현재 단계, {1} = 최대 단계")]
        [SerializeField] string levelFormat = "단계 {0} / {1}";
        [Tooltip("다음 단계 효과. {0} = 능력 이름, {1} = 지금 보너스, {2} = 강화 후 보너스")]
        [SerializeField] string effectFormat = "{0} +{1} → +{2}";
        [Tooltip("최대 단계일 때. {0} = 능력 이름, {1} = 지금 보너스")]
        [SerializeField] string maxEffectFormat = "{0} +{1}";
        [Tooltip("{0} = 다음 단계 가격")]
        [SerializeField] string priceFormat = "{0} G";
        [SerializeField] string maxPriceText = "-";
        [SerializeField] string notEnoughMoneyText = "돈이 모자라요";
        [SerializeField] string maxLevelText = "최대 단계";

        [Header("안내 문구")]
        [Tooltip("{0} = 강화 이름, {1} = 새 단계, {2} = 낸 돈")]
        [SerializeField] string upgradedFormat = "{0} {1}단계 강화 완료! (-{2} G)";

        readonly List<ShopUpgradeRow> rows = new List<ShopUpgradeRow>();

        PlayerUpgrades Upgrades => purchaser != null ? purchaser.Upgrades : null;

        void OnEnable()
        {
            if (wallet != null) wallet.MoneyChanged += HandleMoney;
            if (Upgrades != null) Upgrades.Changed += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            if (wallet != null) wallet.MoneyChanged -= HandleMoney;
            if (Upgrades != null) Upgrades.Changed -= Rebuild;
        }

        void HandleMoney(int _) => Rebuild();

        void Buy(ShopUpgradeData upgrade)
        {
            if (purchaser == null) return;
            int price = purchaser.NextPrice(upgrade);
            if (purchaser.TryBuy(upgrade))
            {
                if (toast != null) toast.Show(string.Format(upgradedFormat, upgrade.DisplayName, Upgrades.GetLevel(upgrade), price), ShopFeedbackKind.Upgrade);
            }
            else if (toast != null)
            {
                var block = purchaser.Check(upgrade);
                toast.Show(block == UpgradePurchaser.Block.MaxLevel ? maxLevelText : notEnoughMoneyText, ShopFeedbackKind.Fail);
            }
        }

        void Rebuild()
        {
            foreach (var row in rows) if (row != null) Destroy(row.gameObject);
            rows.Clear();
            if (catalog == null || rowPrefab == null || listRoot == null || purchaser == null || Upgrades == null) return;

            foreach (var upgrade in catalog.Upgrades)
            {
                if (upgrade == null) continue;
                int level = Upgrades.GetLevel(upgrade);
                bool maxed = Upgrades.IsMaxed(upgrade);
                string statName = UpgradeStatNames.Get(upgrade.Stat);
                string effect = maxed
                    ? string.Format(maxEffectFormat, statName, upgrade.BonusAt(level))
                    : string.Format(effectFormat, statName, upgrade.BonusAt(level), upgrade.BonusAt(level + 1));
                var block = purchaser.Check(upgrade);
                string reason = block == UpgradePurchaser.Block.MaxLevel ? maxLevelText
                    : block == UpgradePurchaser.Block.NotEnoughMoney ? notEnoughMoneyText : "";
                string price = maxed ? maxPriceText : string.Format(priceFormat, purchaser.NextPrice(upgrade));

                var row = Instantiate(rowPrefab, listRoot);
                row.Setup(upgrade, string.Format(levelFormat, level, upgrade.MaxLevel), effect, price, reason,
                    block == UpgradePurchaser.Block.None, Buy);
                rows.Add(row);
            }
        }
    }
}
