using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>재료 판매: 인벤토리에서 재료를 빼고 판매가 × 개수만큼 돈을 더한다.</summary>
    public class MaterialSeller : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("재료를 보관하는 인벤토리")]
        [SerializeField] PlayerInventory inventory;
        [Tooltip("돈을 받을 지갑")]
        [SerializeField] PlayerWallet wallet;

        /// <summary>팔았을 때 (재료, 개수, 받은 돈)</summary>
        public event Action<MaterialData, int, int> Sold;

        public PlayerInventory Inventory => inventory;

        /// <summary>재료를 count개 판다. 받은 돈을 돌려준다 (못 팔면 0).</summary>
        public int Sell(MaterialData material, int count)
        {
            if (material == null || inventory == null || wallet == null || count <= 0) return 0;
            if (!inventory.TryRemove(material, count)) return 0;
            int money = material.SellPrice * count;
            wallet.Add(money);
            Sold?.Invoke(material, count, money);
            return money;
        }

        /// <summary>가진 재료를 모두 판다.</summary>
        public int SellAll(MaterialData material) =>
            inventory != null ? Sell(material, inventory.GetCount(material)) : 0;

        /// <summary>한 희귀도의 재료를 모두 판다. (판 개수, 받은 돈)</summary>
        public (int count, int money) SellAllOfRarity(MaterialRarity rarity)
        {
            if (inventory == null) return (0, 0);
            var targets = new List<MaterialData>();
            foreach (var pair in inventory.Counts)
                if (pair.Key != null && pair.Key.Rarity == rarity) targets.Add(pair.Key);
            int count = 0, money = 0;
            foreach (var material in targets)
            {
                int n = inventory.GetCount(material);
                int got = Sell(material, n);
                if (got > 0 || material.SellPrice == 0) count += n;
                money += got;
            }
            return (count, money);
        }
    }
}
