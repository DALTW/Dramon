using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>아이템 구매: 돈을 빼고 아이템 가방에 넣는다. 상점 재고(무제한 가능)를 관리한다.</summary>
    public class ItemPurchaser : MonoBehaviour
    {
        /// <summary>살 수 없는 이유</summary>
        public enum Block { None, SoldOut, BagFull, NotEnoughMoney, Missing }

        [Header("연결")]
        [Tooltip("아이템을 넣을 가방")]
        [SerializeField] PlayerItemBag bag;
        [Tooltip("돈을 낼 지갑")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("다시 시작을 알려 주는 단계 관리자 (다시 시작하면 재고를 처음으로)")]
        [SerializeField] GamePhaseController phaseController;

        /// <summary>샀을 때 (아이템, 낸 돈)</summary>
        public event Action<ShopItemData, int> Purchased;

        readonly Dictionary<ShopItemData, int> sold = new Dictionary<ShopItemData, int>();

        public PlayerItemBag Bag => bag;

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += sold.Clear;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= sold.Clear;
        }

        /// <summary>남은 상점 재고 (무제한이면 -1)</summary>
        public int RemainingStock(ShopItemData item)
        {
            if (item == null) return 0;
            if (item.UnlimitedStock) return -1;
            sold.TryGetValue(item, out int n);
            return Mathf.Max(0, item.Stock - n);
        }

        public Block Check(ShopItemData item)
        {
            if (item == null || bag == null || wallet == null) return Block.Missing;
            if (RemainingStock(item) == 0) return Block.SoldOut;
            if (bag.RoomFor(item) <= 0) return Block.BagFull;
            if (wallet.Money < item.Price) return Block.NotEnoughMoney;
            return Block.None;
        }

        /// <summary>하나 산다. 성공하면 true.</summary>
        public bool TryBuy(ShopItemData item)
        {
            if (Check(item) != Block.None) return false;
            if (!wallet.TrySpend(item.Price)) return false;
            bag.Add(item, 1);
            sold.TryGetValue(item, out int n);
            sold[item] = n + 1;
            Purchased?.Invoke(item, item.Price);
            return true;
        }
    }
}
