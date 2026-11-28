using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 카드의 최종 수치: 카드 강화 → 능력 강화(검 단련·공격력·방패 보강) → 공포(공격 −25%, 내림) / 젖음(방어 −50%).
    /// 산미 추가 피해와 드래곤 방어는 때리는 순간에 CardEffectResolver가 더하고 뺀다.
    /// </summary>
    public class CardStatCalculator : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("카드 종류 강화 상태")]
        [SerializeField] CardUpgradeState cardUpgrades;
        [Tooltip("능력 강화 (검 단련·공격력 증가·방패 보강 — 지금 상점에서는 팔지 않지만 값이 있으면 반영)")]
        [SerializeField] PlayerUpgrades playerUpgrades;
        [Tooltip("플레이어 상태 효과 (공포·젖음)")]
        [SerializeField] StatusEffects playerStatus;

        [Header("상태 효과 비율")]
        [Tooltip("공포일 때 내 공격 피해 배율 (0.75 = −25%, 내림)")]
        [Range(0f, 1f)] [SerializeField] float fearDamageMultiplier = 0.75f;
        [Tooltip("젖음일 때 얻는 방어 배율 (0.5 = −50%, 내림)")]
        [Range(0f, 1f)] [SerializeField] float drenchedBlockMultiplier = 0.5f;

        public bool IsUpgraded(CardData card) => cardUpgrades != null && cardUpgrades.IsUpgraded(card);

        public int Cost(CardData card) => card != null ? card.CostFor(IsUpgraded(card)) : 0;

        /// <summary>강화만 반영한 효과 (상태 효과 전)</summary>
        public CardEffect BaseEffect(CardData card, int i) => card.EffectFor(i, IsUpgraded(card));

        /// <summary>공격 피해 한 번의 최종 값 (산미·드래곤 방어 전)</summary>
        public int AttackDamage(CardData card, int raw)
        {
            int v = raw;
            if (playerUpgrades != null)
            {
                if (card != null && card.HasTag(CardTag.Sword)) v += playerUpgrades.GetBonus(UpgradeStat.SwordAttack);
                v += playerUpgrades.GetBonus(UpgradeStat.AttackPower);
            }
            if (playerStatus != null && playerStatus.Has(StatusType.Fear)) v = Mathf.FloorToInt(v * fearDamageMultiplier);
            return Mathf.Max(0, v);
        }

        /// <summary>얻는 방어의 최종 값</summary>
        public int Block(int raw)
        {
            int v = raw;
            if (playerUpgrades != null) v += playerUpgrades.GetBonus(UpgradeStat.ShieldDefense);
            if (playerStatus != null && playerStatus.Has(StatusType.Drenched)) v = Mathf.FloorToInt(v * drenchedBlockMultiplier);
            return Mathf.Max(0, v);
        }

        /// <summary>카드 설명에 넣을 효과별 최종 수치</summary>
        public List<int> DisplayValues(CardData card) => DisplayValues(card, IsUpgraded(card));

        /// <summary>강화 여부를 정해서 계산한 효과별 최종 수치 (상점 강화 미리보기용)</summary>
        public List<int> DisplayValues(CardData card, bool upgraded)
        {
            var list = new List<int>();
            if (card == null) return list;
            for (int i = 0; i < card.Effects.Count; i++)
            {
                var e = card.EffectFor(i, upgraded);
                switch (e.type)
                {
                    case CardEffectType.Damage:
                    case CardEffectType.MultiDamage:
                        list.Add(AttackDamage(card, e.value));
                        break;
                    case CardEffectType.Block:
                        list.Add(Block(e.value));
                        break;
                    default:
                        list.Add(e.value);
                        break;
                }
            }
            return list;
        }

        /// <summary>카드 설명 (최종 수치, 바뀐 숫자는 초록색)</summary>
        public string Describe(CardData card) => DescribeAs(card, IsUpgraded(card));

        /// <summary>강화 여부를 정해서 만든 카드 설명 (상점 강화 탭의 "지금 → 강화 후")</summary>
        public string DescribeAs(CardData card, bool upgraded)
        {
            if (card == null) return "";
            var finals = DisplayValues(card, upgraded);
            var counts = new List<int>();
            for (int i = 0; i < card.Effects.Count; i++) counts.Add(card.EffectFor(i, upgraded).count);
            return CardText.Format(card, finals, counts);
        }
    }
}
