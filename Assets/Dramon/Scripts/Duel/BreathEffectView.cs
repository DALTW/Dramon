using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 드래곤 숨결 이펙트: 숨결 공격 때 흰 숨결 프레임(FX_Breath)을 그 드래곤의 속성 색으로 칠해서 한 번 보여 준다.
    /// 색은 드래곤의 플레이어 피격 반응(화상·감전·빙결…)으로 골라요.
    /// </summary>
    public class BreathEffectView : MonoBehaviour
    {
        [Serializable]
        public class Tint
        {
            [Tooltip("드래곤의 플레이어 피격 반응 (DragonSpecies의 Player Hit Reaction)")]
            public HeroReactionType reaction;
            [Tooltip("숨결 색 (흰 그림에 곱해져요)")]
            public Color color = Color.white;
        }

        [Header("연결")]
        [Tooltip("숨결 프레임 재생기 (DuelScreen/Panel/BreathFX)")]
        [SerializeField] UIFramePlayer player;

        [Header("색")]
        [Tooltip("목록에 없는 반응일 때 색")]
        [SerializeField] Color defaultColor = new Color(1f, 0.55f, 0.2f, 1f);
        [Tooltip("반응별 숨결 색")]
        [SerializeField] List<Tint> tints = new List<Tint>
        {
            new Tint { reaction = HeroReactionType.Burned, color = new Color(1f, 0.5f, 0.15f, 1f) },
            new Tint { reaction = HeroReactionType.Shocked, color = new Color(1f, 0.95f, 0.4f, 1f) },
            new Tint { reaction = HeroReactionType.Frozen, color = new Color(0.65f, 0.9f, 1f, 1f) },
            new Tint { reaction = HeroReactionType.Poisoned, color = new Color(0.6f, 0.95f, 0.3f, 1f) },
            new Tint { reaction = HeroReactionType.Drenched, color = new Color(0.45f, 0.75f, 1f, 1f) },
            new Tint { reaction = HeroReactionType.StoneHit, color = new Color(0.8f, 0.7f, 0.55f, 1f) },
            new Tint { reaction = HeroReactionType.Fear, color = new Color(0.65f, 0.45f, 0.9f, 1f) },
            new Tint { reaction = HeroReactionType.CrystalShards, color = new Color(1f, 0.65f, 0.9f, 1f) },
        };

        /// <summary>그 드래곤 색으로 숨결을 한 번 보여 준다</summary>
        public void Play(DragonSpecies species)
        {
            if (player != null) player.Play(ColorFor(species));
        }

        public void Stop()
        {
            if (player != null) player.Stop();
        }

        Color ColorFor(DragonSpecies species)
        {
            if (species == null) return defaultColor;
            foreach (var t in tints)
                if (t != null && t.reaction == species.PlayerHitReaction) return t.color;
            return defaultColor;
        }
    }
}
