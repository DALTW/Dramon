using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>내 턴: 시작(방어 초기화·에너지 채우기·지속 효과·뽑기), 카드 사용, 끝(남은 손패 버리기)</summary>
    public class PlayerTurn : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] PlayerBattleStats stats;
        [SerializeField] DrawPile drawPile;
        [SerializeField] DiscardPile discardPile;
        [SerializeField] ExhaustPile exhaustPile;
        [SerializeField] Hand hand;
        [SerializeField] CardEffectResolver resolver;
        [Tooltip("강화가 반영된 코스트")]
        [SerializeField] CardStatCalculator calculator;
        [Tooltip("감전·빙결·출혈 처리")]
        [SerializeField] StatusEffectProcessor statusProcessor;
        [Tooltip("드래곤 상태 효과 (지속 효과로 산미를 줄 때)")]
        [SerializeField] StatusEffects dragonStatus;
        [Tooltip("카드 동작 모션")]
        [SerializeField] BattleMotionLink motions;
        [Tooltip("떠오르는 숫자")]
        [SerializeField] FloatingNumberUI floating;
        [Tooltip("진행 문구")]
        [SerializeField] BattleLogMessage log;

        int sourAura;

        public bool IsActive { get; private set; }

        /// <summary>카드를 냈을 때</summary>
        public event Action<CardInstance, CardEffectResolver.Result> CardPlayed;

        /// <summary>결투 시작: 지속 효과 초기화</summary>
        public void ResetForDuel()
        {
            sourAura = 0;
            IsActive = false;
        }

        /// <summary>지속 효과 (상큼한 기운): 내 턴 시작마다 드래곤에게 산미</summary>
        public void AddSourAura(int amount) => sourAura += Mathf.Max(0, amount);

        /// <summary>내 턴 시작</summary>
        public void BeginTurn(int maxEnergy, int draw)
        {
            var (energyPenalty, drawPenalty) = statusProcessor != null ? statusProcessor.PlayerTurnStartPenalty() : (0, 0);
            if (stats != null)
            {
                stats.ClearBlock();
                stats.RefillEnergy(maxEnergy);
                if (energyPenalty > 0) stats.AddEnergy(-energyPenalty);
            }
            if (sourAura > 0 && dragonStatus != null)
            {
                dragonStatus.Add(StatusType.Sour, sourAura);
                if (floating != null) floating.OnDragon($"산미 +{sourAura}", FloatingNumberUI.Kind.Status);
            }
            Draw(Mathf.Max(0, draw - drawPenalty));
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

        public int CostOf(CardInstance card) =>
            card == null || card.Data == null ? 0 : calculator != null ? calculator.Cost(card.Data) : card.Data.Cost;

        public bool CanPlay(CardInstance card) =>
            IsActive && card != null && card.Data != null && stats != null && stats.Energy >= CostOf(card) && Contains(card);

        /// <summary>카드를 낸다. 성공하면 true</summary>
        public bool TryPlay(CardInstance card)
        {
            if (!CanPlay(card)) return false;
            stats.TrySpendEnergy(CostOf(card));
            hand.Remove(card);
            if (motions != null) motions.PlayCard(card.Data);
            var result = resolver != null ? resolver.Apply(card.Data) : default;
            if (card.Data.Exhaust && exhaustPile != null) exhaustPile.Add(card);
            else discardPile.Add(card);
            if (log != null) log.Show(CardEffectResolver.Describe(card.Data, result));
            if (statusProcessor != null) statusProcessor.OnCardPlayed(); // 출혈
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
    }
}
