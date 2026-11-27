using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>결투 중 카드 더미 하나 (뽑을 더미·버린 더미·손패·소멸 더미의 공통 부분)</summary>
    public abstract class CardPile : MonoBehaviour
    {
        [Header("확인용 (실행 중)")]
        [Tooltip("실행 중 이 더미에 있는 카드 이름 (확인용)")]
        [SerializeField] List<string> currentCards = new List<string>();

        protected readonly List<CardInstance> cards = new List<CardInstance>();

        /// <summary>카드가 들어오거나 나갈 때</summary>
        public event Action Changed;

        public int Count => cards.Count;
        public IReadOnlyList<CardInstance> Cards => cards;

        public virtual void Add(CardInstance card)
        {
            if (card == null) return;
            cards.Add(card);
            Notify();
        }

        public bool Remove(CardInstance card)
        {
            if (!cards.Remove(card)) return false;
            Notify();
            return true;
        }

        /// <summary>모든 카드를 빼서 돌려준다</summary>
        public List<CardInstance> TakeAll()
        {
            var all = new List<CardInstance>(cards);
            cards.Clear();
            Notify();
            return all;
        }

        public void Clear()
        {
            if (cards.Count == 0) return;
            cards.Clear();
            Notify();
        }

        protected void Notify()
        {
            currentCards.Clear();
            foreach (var c in cards) currentCards.Add(c.Data != null ? c.Data.DisplayName : "?");
            Changed?.Invoke();
        }
    }
}
