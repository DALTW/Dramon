using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>능력 강화 구매: 다음 단계 가격만큼 돈을 빼고 강화 단계를 올린다.</summary>
    public class UpgradePurchaser : MonoBehaviour
    {
        /// <summary>강화할 수 없는 이유</summary>
        public enum Block { None, MaxLevel, NotEnoughMoney, Missing }

        [Header("연결")]
        [Tooltip("강화 단계를 보관하는 곳")]
        [SerializeField] PlayerUpgrades upgrades;
        [Tooltip("돈을 낼 지갑")]
        [SerializeField] PlayerWallet wallet;

        /// <summary>강화했을 때 (강화, 새 단계, 낸 돈)</summary>
        public event Action<ShopUpgradeData, int, int> Purchased;

        public PlayerUpgrades Upgrades => upgrades;

        /// <summary>다음 단계 가격 (최대 단계면 0)</summary>
        public int NextPrice(ShopUpgradeData upgrade)
        {
            if (upgrade == null || upgrades == null || upgrades.IsMaxed(upgrade)) return 0;
            return upgrade.PriceForLevel(upgrades.GetLevel(upgrade) + 1);
        }

        public Block Check(ShopUpgradeData upgrade)
        {
            if (upgrade == null || upgrades == null || wallet == null) return Block.Missing;
            if (upgrades.IsMaxed(upgrade)) return Block.MaxLevel;
            if (wallet.Money < NextPrice(upgrade)) return Block.NotEnoughMoney;
            return Block.None;
        }

        /// <summary>한 단계 강화한다. 성공하면 true.</summary>
        public bool TryBuy(ShopUpgradeData upgrade)
        {
            if (Check(upgrade) != Block.None) return false;
            int price = NextPrice(upgrade);
            if (!wallet.TrySpend(price)) return false;
            upgrades.LevelUp(upgrade);
            Purchased?.Invoke(upgrade, upgrades.GetLevel(upgrade), price);
            return true;
        }
    }
}
