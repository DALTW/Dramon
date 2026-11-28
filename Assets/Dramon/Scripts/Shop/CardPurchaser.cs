using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 구매: 카드의 상점 가격만큼 돈을 빼고 덱에 1장 넣는다. 카드마다 최대 보유 수(Max Owned)를 넘으면 못 산다.</summary>
    public class CardPurchaser : MonoBehaviour
    {
        public enum Block { None, NotEnoughMoney, MaxOwned, NotForSale, Missing }

        [Header("연결")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("산 카드를 넣을 덱")]
        [SerializeField] PlayerDeck deck;

        /// <summary>샀을 때 (카드, 낸 돈)</summary>
        public event Action<CardData, int> Purchased;

        public PlayerDeck Deck => deck;
        public PlayerWallet Wallet => wallet;

        public Block Check(CardData card)
        {
            if (card == null || wallet == null || deck == null) return Block.Missing;
            if (card.ShopPrice <= 0) return Block.NotForSale;
            if (card.MaxOwned > 0 && deck.CountOf(card) >= card.MaxOwned) return Block.MaxOwned;
            if (wallet.Money < card.ShopPrice) return Block.NotEnoughMoney;
            return Block.None;
        }

        public bool TryBuy(CardData card)
        {
            if (Check(card) != Block.None) return false;
            if (!wallet.TrySpend(card.ShopPrice)) return false;
            deck.AddPurchased(card);
            Purchased?.Invoke(card, card.ShopPrice);
            return true;
        }
    }
}
