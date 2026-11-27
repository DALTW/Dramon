using System;
using UnityEngine;

namespace Dramon
{
    /// <summary>카드 종류 (틀 색)</summary>
    public enum CardType
    {
        [InspectorName("공격")] Attack,
        [InspectorName("방어")] Defense,
        [InspectorName("특수")] Special,
        [InspectorName("지속")] Power,
    }

    /// <summary>카드 태그 (강화 연결용, 여러 개 선택 가능)</summary>
    [Flags]
    public enum CardTag
    {
        [InspectorName("없음")] None = 0,
        [InspectorName("검")] Sword = 1,
        [InspectorName("레몬")] Lemon = 2,
    }

    /// <summary>카드를 낼 때 검사가 보일 동작</summary>
    public enum HeroCardAction
    {
        [InspectorName("없음")] None,
        [InspectorName("베기")] Slash,
        [InspectorName("방패")] Block,
        [InspectorName("구르기")] Roll,
        [InspectorName("레몬 던지기")] LemonThrow,
        [InspectorName("레몬즙")] LemonSqueeze,
        [InspectorName("약초")] Herb,
        [InspectorName("카드 뽑기")] DrawCard,
    }

    /// <summary>카드 효과 종류</summary>
    public enum CardEffectType
    {
        [InspectorName("피해")] Damage,
        [InspectorName("여러 번 피해 (수치 × 횟수)")] MultiDamage,
        [InspectorName("방어")] Block,
        [InspectorName("산미")] Sour,
        [InspectorName("회복")] Heal,
        [InspectorName("카드 뽑기")] Draw,
        [InspectorName("에너지")] Energy,
        [InspectorName("반사 (이번 드래곤 턴)")] Reflect,
        [InspectorName("산미 비례 피해 (산미 × 수치)")] SourScaledDamage,
        [InspectorName("산미 있을 때 추가 피해")] BonusDamageIfSour,
        [InspectorName("지속: 내 턴마다 산미")] SourEachTurn,
    }

    /// <summary>카드 효과 하나 (종류 + 수치 + 횟수)</summary>
    [Serializable]
    public class CardEffect
    {
        public CardEffectType type;
        [Tooltip("수치 (피해·방어·산미·회복·뽑기 장수 등)")]
        public int value;
        [Tooltip("횟수 (여러 번 피해에서만 사용)")]
        [Min(1)]
        public int count = 1;

        public CardEffect() { }
        public CardEffect(CardEffectType type, int value, int count = 1) { this.type = type; this.value = value; this.count = count; }
    }

    /// <summary>상태 효과 종류</summary>
    public enum StatusType
    {
        [InspectorName("없음")] None,
        [InspectorName("산미")] Sour,
        [InspectorName("화상")] Burn,
        [InspectorName("중독")] Poison,
        [InspectorName("감전")] Shock,
        [InspectorName("빙결")] Frozen,
        [InspectorName("젖음")] Drenched,
        [InspectorName("공포")] Fear,
        [InspectorName("출혈")] Bleed,
    }

    /// <summary>드래곤 행동 종류</summary>
    public enum DragonActionType
    {
        [InspectorName("할퀴기 (피해)")] Claw,
        [InspectorName("연타 (피해 × 횟수)")] MultiHit,
        [InspectorName("숨결 (피해 + 상태)")] Breath,
        [InspectorName("방어")] Defend,
        [InspectorName("회복")] Heal,
        [InspectorName("강화 (다음 공격 + 피해)")] Buff,
    }
}
