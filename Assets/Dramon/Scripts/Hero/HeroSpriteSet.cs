using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>검사 동작 하나: 프레임 배열과 초당 프레임 수. 한 장짜리 동작은 1 ÷ 초당 프레임 수(초) 동안 보여요.</summary>
    [Serializable]
    public class HeroClip
    {
        [Tooltip("순서대로 재생할 프레임")]
        public Sprite[] frames = new Sprite[0];
        [Tooltip("초당 프레임 수 (한 장짜리면 1 ÷ 이 값 = 보여 주는 시간)")]
        [Min(0.1f)]
        public float fps = 6f;

        public HeroClip() { }
        public HeroClip(float fps) { this.fps = fps; }

        /// <summary>비어 있지 않은 프레임만 (없으면 null)</summary>
        public Sprite[] Valid()
        {
            if (frames == null) return null;
            var list = new List<Sprite>();
            foreach (var f in frames) if (f != null) list.Add(f);
            return list.Count > 0 ? list.ToArray() : null;
        }
    }

    /// <summary>
    /// 결투 화면 검사(레몬 검사)의 동작별 프레임 묶음. Inspector에서 프레임을 바꾸거나 넣고 뺄 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroSpriteSet", menuName = "Dramon/Hero Sprite Set")]
    public class HeroSpriteSet : ScriptableObject
    {
        [Header("대기")]
        [Tooltip("결투 중 반복할 대기 프레임. 기본은 검이 보이는 Idle_0, Idle_3. Idle_1·2(숨 고르기)를 넣어 섞을 수도 있어요")]
        [SerializeField] HeroClip idle = new HeroClip(3f);
        [Tooltip("체력이 낮을 때 대기 대신 보여 줄 자세")]
        [SerializeField] HeroClip lowHealth = new HeroClip(2f);
        [Tooltip("등장할 때(슬라이드 인 중) 반복할 걷기")]
        [SerializeField] HeroClip enter = new HeroClip(6f);

        [Header("카드 동작")]
        [Tooltip("베기 (준비 → 베기 → 마무리 → 복귀)")]
        [SerializeField] HeroClip slash = new HeroClip(10f);
        [Tooltip("방패 (들기 → 막는 순간)")]
        [SerializeField] HeroClip block = new HeroClip(5f);
        [Tooltip("구르기 (구르기 → 착지)")]
        [SerializeField] HeroClip roll = new HeroClip(5f);
        [Tooltip("레몬 던지기 (꺼내기 → 던지기)")]
        [SerializeField] HeroClip lemonThrow = new HeroClip(5f);
        [Tooltip("레몬즙")]
        [SerializeField] HeroClip lemonSqueeze = new HeroClip(2f);
        [Tooltip("약초")]
        [SerializeField] HeroClip herb = new HeroClip(2f);
        [Tooltip("카드 뽑기")]
        [SerializeField] HeroClip drawCard = new HeroClip(2f);

        [Header("결과")]
        [Tooltip("피격 순간")]
        [SerializeField] HeroClip hit = new HeroClip(4f);
        [Tooltip("피격 뒤 자세 회복 (피격·속성 반응 다음에 나와요)")]
        [SerializeField] HeroClip hitRecover = new HeroClip(3f);
        [Tooltip("승리 (전리품 창까지 유지)")]
        [SerializeField] HeroClip victory = new HeroClip(2f);
        [Tooltip("쓰러짐 (결투 패배)")]
        [SerializeField] HeroClip defeated = new HeroClip(2f);

        [Header("속성 반응 (드래곤 브레스)")]
        [SerializeField] HeroClip burned = new HeroClip(2f);
        [SerializeField] HeroClip shocked = new HeroClip(2f);
        [SerializeField] HeroClip frozen = new HeroClip(2f);
        [SerializeField] HeroClip poisoned = new HeroClip(2f);
        [SerializeField] HeroClip drenched = new HeroClip(2f);
        [SerializeField] HeroClip stoneHit = new HeroClip(2f);
        [SerializeField] HeroClip fear = new HeroClip(2f);
        [SerializeField] HeroClip crystalShards = new HeroClip(2f);

        public HeroClip Idle => idle;
        public HeroClip LowHealth => lowHealth;
        public HeroClip Enter => enter;
        public HeroClip Slash => slash;
        public HeroClip Block => block;
        public HeroClip Roll => roll;
        public HeroClip LemonThrow => lemonThrow;
        public HeroClip LemonSqueeze => lemonSqueeze;
        public HeroClip Herb => herb;
        public HeroClip DrawCard => drawCard;
        public HeroClip Hit => hit;
        public HeroClip HitRecover => hitRecover;
        public HeroClip Victory => victory;
        public HeroClip Defeated => defeated;

        /// <summary>속성 반응 동작 (없음이면 기본 피격)</summary>
        public HeroClip Reaction(HeroReactionType type)
        {
            switch (type)
            {
                case HeroReactionType.Burned: return burned;
                case HeroReactionType.Shocked: return shocked;
                case HeroReactionType.Frozen: return frozen;
                case HeroReactionType.Poisoned: return poisoned;
                case HeroReactionType.Drenched: return drenched;
                case HeroReactionType.StoneHit: return stoneHit;
                case HeroReactionType.Fear: return fear;
                case HeroReactionType.CrystalShards: return crystalShards;
                default: return hit;
            }
        }
    }
}
