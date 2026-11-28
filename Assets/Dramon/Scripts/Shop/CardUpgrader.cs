using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 강화: 카드 종류 단위로 1단계("+"). 강화 가격만큼 돈을 빼고 CardUpgradeState에 기록한다 (덱의 그 카드 모두에 적용).</summary>
    public class CardUpgrader : MonoBehaviour
    {
        public enum Block { None, NotEnoughMoney, AlreadyUpgraded, NotUpgradable, Missing }

        [Header("연결")]
        [SerializeField] PlayerWallet wallet;
        [Tooltip("카드 강화 상태")]
        [SerializeField] CardUpgradeState upgrades;
        [Tooltip("강화할 카드 목록을 가져올 덱")]
        [SerializeField] PlayerDeck deck;

        /// <summary>강화했을 때 (카드, 낸 돈)</summary>
        public event Action<CardData, int> Upgraded;

        public PlayerDeck Deck => deck;
        public PlayerWallet Wallet => wallet;
        public CardUpgradeState State => upgrades;

        public bool IsUpgraded(CardData card) => upgrades != null && upgrades.IsUpgraded(card);

        public Block Check(CardData card)
        {
            if (card == null || wallet == null || upgrades == null) return Block.Missing;
            if (!card.Upgradable) return Block.NotUpgradable;
            if (upgrades.IsUpgraded(card)) return Block.AlreadyUpgraded;
            if (wallet.Money < card.UpgradePrice) return Block.NotEnoughMoney;
            return Block.None;
        }

        public bool TryUpgrade(CardData card)
        {
            if (Check(card) != Block.None) return false;
            if (!wallet.TrySpend(card.UpgradePrice)) return false;
            upgrades.Upgrade(card);
            Upgraded?.Invoke(card, card.UpgradePrice);
            return true;
        }
    }
}
