using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>내 턴: 시작(방어 초기화·에너지 채우기·뽑기), 카드 사용, 끝(남은 손패 버리기)</summary>
    public class PlayerTurn : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] PlayerBattleStats stats;
        [SerializeField] DrawPile drawPile;
        [SerializeField] DiscardPile discardPile;
        [SerializeField] ExhaustPile exhaustPile;
        [SerializeField] Hand hand;
        [SerializeField] CardEffectResolver resolver;
        [Tooltip("진행 문구")]
        [SerializeField] BattleLogMessage log;

        [Header("문구")]
        [Tooltip("{0} = 카드 이름, {1} = 피해")]
        [SerializeField] string damageFormat = "{0}! 드래곤에게 {1} 피해";
        [Tooltip("{0} = 카드 이름, {1} = 방어")]
        [SerializeField] string blockFormat = "{0}! 방어 {1}";
        [Tooltip("{0} = 카드 이름")]
        [SerializeField] string usedFormat = "{0}!";

        public bool IsActive { get; private set; }

        /// <summary>카드를 냈을 때</summary>
        public event Action<CardInstance, CardEffectResolver.Result> CardPlayed;

        /// <summary>내 턴 시작</summary>
        public void BeginTurn(int energy, int draw)
        {
            if (stats != null)
            {
                stats.ClearBlock();
                stats.RefillEnergy(energy);
            }
            Draw(draw);
            IsActive = true;
        }

        /// <summary>n장 뽑는다 (뽑을 더미가 비면 버린 더미를 섞어서). 손패가 가득이면 버린 더미로. 실제로 손패에 들어온 장수를 돌려준다</summary>
        public int Draw(int n)
        {
            int drawn = 0;
            for (int i = 0; i < n; i++)
            {
                if (drawPile.Count == 0 && discardPile.Count > 0) drawPile.AddAndShuffle(discardPile.TakeAll());
                var card = drawPile.DrawTop();
                if (card == null) break;
                if (hand.IsFull) discardPile.Add(card);
                else { hand.Add(card); drawn++; }
            }
            return drawn;
        }

        public bool CanPlay(CardInstance card) =>
            IsActive && card != null && card.Data != null && stats != null && stats.Energy >= card.Data.Cost && Contains(card);

        /// <summary>카드를 낸다. 성공하면 true</summary>
        public bool TryPlay(CardInstance card)
        {
            if (!CanPlay(card)) return false;
            stats.TrySpendEnergy(card.Data.Cost);
            hand.Remove(card);
            var result = resolver != null ? resolver.Apply(card.Data) : default;
            if (card.Data.Exhaust && exhaustPile != null) exhaustPile.Add(card);
            else discardPile.Add(card);
            Log(card.Data, result);
            CardPlayed?.Invoke(card, result);
            return true;
        }

        /// <summary>내 턴 끝: 남은 손패를 버린 더미로</summary>
        public void EndTurn()
        {
            IsActive = false;
            foreach (var c in hand.TakeAll()) discardPile.Add(c);
        }

        /// <summary>결투가 끝나 멈춤</summary>
        public void Stop() => IsActive = false;

        bool Contains(CardInstance card)
        {
            foreach (var c in hand.Cards) if (c == card) return true;
            return false;
        }

        void Log(CardData card, CardEffectResolver.Result r)
        {
            if (log == null) return;
            if (r.hits > 0) log.Show(string.Format(damageFormat, card.DisplayName, r.damage));
            else if (r.block > 0) log.Show(string.Format(blockFormat, card.DisplayName, r.block));
            else log.Show(string.Format(usedFormat, card.DisplayName));
        }
    }
}
