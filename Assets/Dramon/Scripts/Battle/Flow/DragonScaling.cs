using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 드래곤 강해지기: 스테이지 배율 × (1 + 같은 스테이지 결투 번호 × 5%), 보스면 보스 배율까지.
    /// 체력과 할퀴기·연타·숨결 피해에만 적용하고, 방어·회복·강화·상태 수치는 그대로 둔다.
    /// </summary>
    public class DragonScaling : MonoBehaviour
    {
        [Header("연결")]
        [Tooltip("스테이지 구성 공식 (배율 표)")]
        [SerializeField] StageRules stageRules;
        [Tooltip("예전 결투 순서 강화(+6%) 규칙. BattleRules에서 켜야만 공격에 더 곱해져요 (지금은 꺼짐)")]
        [SerializeField] BattleRules battleRules;

        [Header("테스트 (에디터에서만)")]
        [Tooltip("0보다 크면 이 스테이지 배율로 강제 (에디터에서만)")]
        [Min(0)] [SerializeField] int testForceStage;

        BossRules boss;

        public int Stage { get; private set; } = 1;
        public int DuelIndexInStage { get; private set; }
        /// <summary>다시 시작한 뒤 몇 번째 결투인지 (0부터, 예전 규칙용)</summary>
        public int TotalDuelIndex { get; private set; }
        public bool IsBoss => boss != null;
        public BossRules Boss => boss;

        int EffectiveStage => Application.isEditor && testForceStage > 0 ? testForceStage : Stage;

        /// <summary>이번 결투의 스테이지와 그 스테이지에서 몇 번째 결투인지(0부터)</summary>
        public void SetDuel(int stage, int duelIndexInStage, int totalDuelIndex = 0)
        {
            Stage = Mathf.Max(1, stage);
            DuelIndexInStage = Mathf.Max(0, duelIndexInStage);
            TotalDuelIndex = Mathf.Max(0, totalDuelIndex);
            boss = null;
        }

        /// <summary>보스전: 그 스테이지(마지막 스테이지) 값 × 보스 배율</summary>
        public void SetBoss(int stage, BossRules rules)
        {
            Stage = Mathf.Max(1, stage);
            DuelIndexInStage = 0;
            boss = rules;
        }

        public float HealthMultiplier =>
            (stageRules != null ? stageRules.HealthMultiplier(EffectiveStage, DuelIndexInStage) : 1f) * (boss != null ? boss.HealthMultiplier : 1f) * LegacyMultiplier;

        public float AttackMultiplier =>
            (stageRules != null ? stageRules.AttackMultiplier(EffectiveStage, DuelIndexInStage) : 1f) * (boss != null ? boss.AttackMultiplier : 1f) * LegacyMultiplier;

        float LegacyMultiplier =>
            battleRules != null && battleRules.DuelScalingEnabled ? 1f + battleRules.ScalingPerDuel * TotalDuelIndex : 1f;

        public int MaxHealth(DragonSpecies species) =>
            species != null ? Mathf.Max(1, Mathf.RoundToInt(species.MaxHealth * HealthMultiplier)) : 1;

        /// <summary>행동 한 번의 피해 (공격 행동만 배율, 나머지는 수치 그대로)</summary>
        public int Value(DragonActionData action)
        {
            if (action == null) return 0;
            return action.IsAttack ? Mathf.RoundToInt(action.Value * AttackMultiplier) : action.Value;
        }
    }
}
