using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 플레이어의 영구 덱: 처음 덱 + 상점에서 산 카드. 결투를 시작할 때 이 목록으로 뽑을 더미를 만든다.
    /// 다시 시작(Restart)하면 산 카드를 비운다.
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

        /// <summary>덱 구성이 바뀔 때</summary>
        public event Action Changed;

        void Awake() => purchasedCards.Clear();

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
            if (starterDeck != null)
                foreach (var e in starterDeck.Entries)
                    if (e.card != null)
                        for (int i = 0; i < e.count; i++) list.Add(e.card);
            foreach (var c in purchasedCards) if (c != null) list.Add(c);
            return list;
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
            if (!clearOnRestart || purchasedCards.Count == 0) return;
            purchasedCards.Clear();
            Changed?.Invoke();
        }
    }
}
