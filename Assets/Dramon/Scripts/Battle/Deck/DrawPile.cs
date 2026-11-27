using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>뽑을 더미: 섞기, 맨 위에서 한 장 뽑기</summary>
    public class DrawPile : CardPile
    {
        /// <summary>카드들을 넣고 섞는다</summary>
        public void AddAndShuffle(IEnumerable<CardInstance> newCards)
        {
            cards.AddRange(newCards);
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
            Notify();
        }

        /// <summary>맨 위 카드 한 장 (비어 있으면 null)</summary>
        public CardInstance DrawTop()
        {
            if (cards.Count == 0) return null;
            var top = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            Notify();
            return top;
        }
    }
}
