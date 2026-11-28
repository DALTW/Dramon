using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 상태 효과가 줄어들고 피해를 주는 시점을 처리한다.
    /// 내 턴 시작: 감전(에너지 −1)·빙결(뽑기 −1) / 카드를 낼 때: 출혈 1 피해 /
    /// 내 턴 끝: 화상(방어로 막음)·중독(방어 무시) 피해, 화상·중독·공포·출혈 1 감소, 감전·빙결·젖음 해제 / 드래곤 턴 끝: 산미 1 감소.
    /// </summary>
    public class StatusEffectProcessor : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] PlayerBattleStats player;
        [Tooltip("플레이어 상태 효과")]
        [SerializeField] StatusEffects playerStatus;
        [Tooltip("드래곤 상태 효과 (산미)")]
        [SerializeField] StatusEffects dragonStatus;
        [Tooltip("떠오르는 숫자")]
        [SerializeField] FloatingNumberUI floating;

        [Header("수치")]
        [Tooltip("감전일 때 줄어드는 에너지")]
        [Min(0)] [SerializeField] int shockEnergyPenalty = 1;
        [Tooltip("빙결일 때 줄어드는 뽑기 장수")]
        [Min(0)] [SerializeField] int frozenDrawPenalty = 1;
        [Tooltip("출혈일 때 카드를 낼 때마다 받는 피해")]
        [Min(0)] [SerializeField] int bleedDamage = 1;

        /// <summary>내 턴 시작: (줄어드는 에너지, 줄어드는 뽑기)</summary>
        public (int energy, int draw) PlayerTurnStartPenalty()
        {
            int e = playerStatus != null && playerStatus.Has(StatusType.Shock) ? shockEnergyPenalty : 0;
            int d = playerStatus != null && playerStatus.Has(StatusType.Frozen) ? frozenDrawPenalty : 0;
            return (e, d);
        }

        /// <summary>카드를 낸 뒤: 출혈 피해. 받은 피해를 돌려준다</summary>
        public int OnCardPlayed()
        {
            if (playerStatus == null || player == null || !playerStatus.Has(StatusType.Bleed)) return 0;
            int lost = player.TakeDirect(bleedDamage);
            if (floating != null && lost > 0) floating.OnPlayer($"-{lost}", FloatingNumberUI.Kind.Damage);
            return lost;
        }

        /// <summary>내 턴 끝: 화상·중독 피해와 상태 감소. 진행 문구 목록을 돌려준다</summary>
        public List<string> OnPlayerTurnEnd()
        {
            var log = new List<string>();
            if (playerStatus == null || player == null) return log;
            int burn = playerStatus.Get(StatusType.Burn);
            if (burn > 0)
            {
                var (_, lost) = player.TakeAttack(burn);
                if (floating != null) floating.OnPlayer($"-{lost}", FloatingNumberUI.Kind.Damage);
                log.Add($"화상으로 {lost} 피해");
            }
            int poison = playerStatus.Get(StatusType.Poison);
            if (poison > 0 && !player.IsDead)
            {
                int lost = player.TakeDirect(poison);
                if (floating != null) floating.OnPlayer($"-{lost}", FloatingNumberUI.Kind.Damage);
                log.Add($"중독으로 {lost} 피해");
            }
            playerStatus.Reduce(StatusType.Burn);
            playerStatus.Reduce(StatusType.Poison);
            playerStatus.Reduce(StatusType.Fear);
            playerStatus.Reduce(StatusType.Bleed);
            playerStatus.Clear(StatusType.Shock);
            playerStatus.Clear(StatusType.Frozen);
            playerStatus.Clear(StatusType.Drenched);
            return log;
        }

        /// <summary>드래곤 턴 끝: 산미 1 감소, 반사 해제</summary>
        public void OnDragonTurnEnd()
        {
            if (dragonStatus != null) dragonStatus.Reduce(StatusType.Sour);
            if (player != null) player.ClearReflect();
        }
    }
}
