using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 카드 효과 처리. 공격 피해 = (카드 피해 + 강화) → 공포 −25%(내림) → + 산미 → 드래곤 방어로 먼저 막음 (연타는 한 번마다).
    /// </summary>
    public class CardEffectResolver : MonoBehaviour
    {
        /// <summary>카드 한 장을 쓴 결과 (메시지용)</summary>
        public struct Result
        {
            public int damage;   // 드래곤 체력에 들어간 피해 합
            public int blocked;  // 드래곤 방어가 막은 양
            public int hits;     // 때린 횟수
            public int block;    // 얻은 방어
            public int drawn;    // 뽑은 장수
            public int sour;     // 준 산미
            public int healed;   // 회복한 체력
            public int reflect;  // 얻은 반사
            public bool negate;  // 다음 공격 무효
            public bool aura;    // 지속 효과 시작
        }

        [Header("연결")]
        [SerializeField] PlayerBattleStats player;
        [SerializeField] DragonBattleStats dragon;
        [Tooltip("카드 뽑기·지속 효과에 씀")]
        [SerializeField] PlayerTurn playerTurn;
        [Tooltip("강화·상태가 반영된 최종 수치")]
        [SerializeField] CardStatCalculator calculator;
        [Tooltip("드래곤 상태 효과 (산미)")]
        [SerializeField] StatusEffects dragonStatus;
        [Tooltip("떠오르는 숫자")]
        [SerializeField] FloatingNumberUI floating;
        [Tooltip("드래곤 피격 모션")]
        [SerializeField] BattleMotionLink motions;

        public Result Apply(CardData card)
        {
            var r = new Result();
            if (card == null) return r;

            // 산미가 있으면 더해지는 피해 (드래곤 슬레이어) → 첫 피해 효과에 더함
            int sourBonus = 0;
            for (int i = 0; i < card.Effects.Count; i++)
            {
                var e = Effect(card, i);
                if (e.type == CardEffectType.BonusDamageIfSour && Sour > 0) sourBonus += e.value;
            }

            for (int i = 0; i < card.Effects.Count; i++)
            {
                if (dragon != null && dragon.IsDead) break;
                var e = Effect(card, i);
                switch (e.type)
                {
                    case CardEffectType.Damage:
                        Hit(card, e.value + sourBonus, ref r);
                        sourBonus = 0;
                        break;
                    case CardEffectType.MultiDamage:
                        for (int k = 0; k < Mathf.Max(1, e.count) && dragon != null && !dragon.IsDead; k++) Hit(card, e.value, ref r);
                        break;
                    case CardEffectType.SourScaledDamage:
                        Hit(card, Sour * e.value, ref r); // 산미는 유지
                        break;
                    case CardEffectType.Block:
                    {
                        int b = calculator != null ? calculator.Block(e.value) : e.value;
                        if (player != null) player.GainBlock(b);
                        r.block += b;
                        if (floating != null && b > 0) floating.OnPlayer($"+{b}", FloatingNumberUI.Kind.Block);
                        break;
                    }
                    case CardEffectType.Sour:
                        if (dragonStatus != null) dragonStatus.Add(StatusType.Sour, e.value);
                        r.sour += e.value;
                        if (floating != null) floating.OnDragon($"산미 +{e.value}", FloatingNumberUI.Kind.Status);
                        break;
                    case CardEffectType.Heal:
                    {
                        int h = player != null ? player.Heal(e.value) : 0;
                        r.healed += h;
                        if (floating != null) floating.OnPlayer($"+{h}", FloatingNumberUI.Kind.Heal);
                        break;
                    }
                    case CardEffectType.Draw:
                        if (playerTurn != null) r.drawn += playerTurn.Draw(e.value);
                        break;
                    case CardEffectType.Energy:
                        if (player != null) player.AddEnergy(e.value);
                        break;
                    case CardEffectType.Reflect:
                        if (player != null) player.AddReflect(e.value);
                        r.reflect += e.value;
                        break;
                    case CardEffectType.SourEachTurn:
                        if (playerTurn != null) playerTurn.AddSourAura(e.value);
                        r.aura = true;
                        break;
                    case CardEffectType.NegateNextAttack:
                        if (player != null) player.AddNegate(1);
                        r.negate = true;
                        break;
                }
            }
            return r;
        }

        int Sour => dragonStatus != null ? dragonStatus.Get(StatusType.Sour) : 0;

        CardEffect Effect(CardData card, int i) => calculator != null ? calculator.BaseEffect(card, i) : card.Effects[i];

        void Hit(CardData card, int raw, ref Result r)
        {
            if (dragon == null) return;
            int dmg = (calculator != null ? calculator.AttackDamage(card, raw) : raw) + Sour;
            var (blocked, lost) = dragon.TakeAttack(dmg);
            r.damage += lost;
            r.blocked += blocked;
            r.hits++;
            if (floating != null)
            {
                if (lost > 0) floating.OnDragon($"-{lost}", FloatingNumberUI.Kind.Damage);
                else if (blocked > 0) floating.OnDragon("막음", FloatingNumberUI.Kind.Block);
            }
            if (motions != null && lost > 0) motions.DragonHurt();
        }

        /// <summary>진행 문구: "검 베기! 드래곤에게 9 피해, 산미 2"</summary>
        public static string Describe(CardData card, Result r)
        {
            var parts = new List<string>();
            if (r.hits > 0) parts.Add($"드래곤에게 {r.damage} 피해" + (r.hits > 1 ? $" ({r.hits}번)" : ""));
            if (r.sour > 0) parts.Add($"산미 {r.sour}");
            if (r.block > 0) parts.Add($"방어 {r.block}");
            if (r.healed > 0) parts.Add($"체력 {r.healed} 회복");
            if (r.reflect > 0) parts.Add($"반사 {r.reflect}");
            if (r.drawn > 0) parts.Add($"카드 {r.drawn}장");
            if (r.negate) parts.Add("다음 공격 무효");
            if (r.aura) parts.Add("내 턴마다 산미");
            string head = card != null ? card.DisplayName : "카드";
            return parts.Count > 0 ? $"{head}! {string.Join(", ", parts)}" : $"{head}!";
        }
    }
}
