using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 제거: 덱에서 카드 1장을 뺀다. 가격은 처음 50골드, 뺄 때마다 +25 (이번 판 기준, 다시 시작하면 처음 가격).</summary>
    public class CardRemover : MonoBehaviour
    {
        public enum Block { None, NotEnoughMoney, DeckTooSmall, Missing }

        [Header("연결")]
        [SerializeField] PlayerWallet wallet;
        [SerializeField] PlayerDeck deck;
        [Tooltip("다시 시작하면 가격을 처음으로")]
        [SerializeField] GamePhaseController phaseController;

        [Header("가격")]
        [Tooltip("처음 제거 가격")]
        [Min(0)] [SerializeField] int basePrice = 50;
        [Tooltip("한 번 뺄 때마다 오르는 가격")]
        [Min(0)] [SerializeField] int priceStep = 25;

        [Header("안전장치")]
        [Tooltip("덱이 이 장수 이하면 더 뺄 수 없어요")]
        [Min(1)] [SerializeField] int minDeckSize = 5;

        /// <summary>뺐을 때 (카드, 낸 돈)</summary>
        public event Action<CardData, int> Removed;

        public int RemovedCount { get; private set; }
        public int Price => basePrice + priceStep * RemovedCount;
        public int MinDeckSize => minDeckSize;
        public PlayerDeck Deck => deck;
        public PlayerWallet Wallet => wallet;

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        void HandleRestarted() => RemovedCount = 0;

        public Block Check(CardData card)
        {
            if (card == null || wallet == null || deck == null || deck.CountOf(card) == 0) return Block.Missing;
            if (deck.Count <= minDeckSize) return Block.DeckTooSmall;
            if (wallet.Money < Price) return Block.NotEnoughMoney;
            return Block.None;
        }

        public bool TryRemove(CardData card)
        {
            if (Check(card) != Block.None) return false;
            int price = Price;
            if (!wallet.TrySpend(price)) return false;
            RemovedCount++; // 목록이 덱 변경을 듣고 다시 그리기 전에 다음 가격으로
            deck.RemoveOne(card);
            Removed?.Invoke(card, price);
            return true;
        }
    }
}
