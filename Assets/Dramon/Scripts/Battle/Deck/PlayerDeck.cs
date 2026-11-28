using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 플레이어의 영구 덱: 처음 덱 + 상점에서 산 카드 − 상점에서 뺀 카드. 결투를 시작할 때 이 목록으로 뽑을 더미를 만든다.
    /// 다시 시작(Restart)하면 산 카드·뺀 카드를 비운다 (처음 덱으로).
    /// </summary>
    public class PlayerDeck : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("처음 덱 구성")]
        [SerializeField] StarterDeck starterDeck;
        [Tooltip("다시 시작을 알려 주는 단계 관리자")]
        [SerializeField] GamePhaseController phaseController;

        [Header("초기화")]
        [Tooltip("켜면 다시 시작할 때 상점에서 산 카드를 비워요")]
        [SerializeField] bool clearOnRestart = true;

        [Header("확인용 (실행 중 산 카드)")]
        [SerializeField] List<CardData> purchasedCards = new List<CardData>();
        [Tooltip("확인용 (실행 중 처음 덱에서 뺀 카드)")]
        [SerializeField] List<CardData> removedStarterCards = new List<CardData>();

        /// <summary>덱 구성이 바뀔 때</summary>
        public event Action Changed;

        void Awake()
        {
            purchasedCards.Clear();
            removedStarterCards.Clear();
        }

        void OnEnable()
        {
            if (phaseController != null) phaseController.Restarted += HandleRestarted;
        }

        void OnDisable()
        {
            if (phaseController != null) phaseController.Restarted -= HandleRestarted;
        }

        /// <summary>지금 덱의 카드 목록 (처음 덱 + 산 카드)</summary>
        public List<CardData> BuildCardList()
        {
            var list = new List<CardData>();
            var removed = new List<CardData>(removedStarterCards);
            if (starterDeck != null)
                foreach (var e in starterDeck.Entries)
                    if (e.card != null)
                        for (int i = 0; i < e.count; i++)
                        {
                            if (removed.Remove(e.card)) continue; // 상점에서 뺀 카드
                            list.Add(e.card);
                        }
            foreach (var c in purchasedCards) if (c != null) list.Add(c);
            return list;
        }

        /// <summary>지금 덱의 카드 수</summary>
        public int Count => BuildCardList().Count;

        /// <summary>덱에 있는 카드 종류 (처음 나온 순서)</summary>
        public List<CardData> DistinctCards()
        {
            var list = new List<CardData>();
            foreach (var c in BuildCardList()) if (!list.Contains(c)) list.Add(c);
            return list;
        }

        /// <summary>카드 1장을 덱에서 뺀다 (산 카드부터, 없으면 처음 덱에서). 뺐으면 true</summary>
        public bool RemoveOne(CardData card)
        {
            if (card == null || CountOf(card) == 0) return false;
            if (!purchasedCards.Remove(card)) removedStarterCards.Add(card);
            Changed?.Invoke();
            return true;
        }

        /// <summary>덱에 이 카드가 몇 장 있는지</summary>
        public int CountOf(CardData card)
        {
            int n = 0;
            foreach (var c in BuildCardList()) if (c == card) n++;
            return n;
        }

        /// <summary>상점에서 산 카드를 덱에 1장 추가</summary>
        public void AddPurchased(CardData card)
        {
            if (card == null) return;
            purchasedCards.Add(card);
            Changed?.Invoke();
        }

        void HandleRestarted()
        {
            if (!clearOnRestart || (purchasedCards.Count == 0 && removedStarterCards.Count == 0)) return;
            purchasedCards.Clear();
            removedStarterCards.Clear();
            Changed?.Invoke();
        }
    }
}
