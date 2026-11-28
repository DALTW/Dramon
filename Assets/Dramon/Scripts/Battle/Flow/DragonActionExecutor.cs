using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 드래곤 행동 실행: 할퀴기·연타·숨결(피해 + 상태)·방어·회복·강화.
    /// 공격 피해 = 행동 수치 × 스테이지 배율 (+ 강화 보너스). 연막 구르기(무효)·반격 자세(반사)도 여기서 처리.
    /// </summary>
    public class DragonActionExecutor : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] PlayerBattleStats player;
        [SerializeField] DragonBattleStats dragon;
        [Tooltip("플레이어 상태 효과 (숨결로 받는 상태)")]
        [SerializeField] StatusEffects playerStatus;
        [Tooltip("스테이지·결투 순서 배율")]
        [SerializeField] DragonScaling scaling;
        [Tooltip("드래곤 공격·검사 피격 모션")]
        [SerializeField] BattleMotionLink motions;
        [Tooltip("떠오르는 숫자")]
        [SerializeField] FloatingNumberUI floating;
        [Tooltip("진행 문구")]
        [SerializeField] BattleLogMessage log;

        [Header("연출 시간")]
        [Tooltip("공격 모션이 시작되고 피해가 들어가기까지(초)")]
        [Min(0f)]
        [SerializeField] float attackLead = 0.25f;
        [Tooltip("연타에서 한 번 때리고 다음까지 기다리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float hitInterval = 0.35f;

        [Header("문구")]
        [Tooltip("{0} = 드래곤 이름, {1} = 행동 이름, {2} = 체력 피해")]
        [SerializeField] string attackFormat = "{0}의 {1}! {2} 피해";
        [Tooltip("방어로 막은 양이 있을 때 덧붙는 문구. {0} = 막은 양")]
        [SerializeField] string blockedSuffix = " ({0} 막음)";
        [Tooltip("숨결 상태가 붙을 때 덧붙는 문구. {0} = 상태 이름, {1} = 수치")]
        [SerializeField] string statusSuffix = ", {0} {1}";
        [Tooltip("{0} = 드래곤 이름, {1} = 행동 이름")]
        [SerializeField] string negatedFormat = "연막 속으로 피했다! {0}의 {1} 무효";
        [Tooltip("반사 피해 문구. {0} = 반사 피해")]
        [SerializeField] string reflectSuffix = ", 반사 {0}";
        [Tooltip("{0} = 드래곤 이름, {1} = 방어")]
        [SerializeField] string defendFormat = "{0}이(가) 몸을 웅크렸다! 방어 {1}";
        [Tooltip("{0} = 드래곤 이름, {1} = 회복량")]
        [SerializeField] string healFormat = "{0}이(가) 상처를 핥았다! 체력 {1} 회복";
        [Tooltip("{0} = 드래곤 이름, {1} = 강화량")]
        [SerializeField] string buffFormat = "{0}이(가) 힘을 모은다! 다음 공격 +{1}";

        /// <summary>그 행동 한 번의 피해·수치 (공격이면 배율, 강화 보너스 포함)</summary>
        public int DamageOf(DragonActionData action)
        {
            if (action == null) return 0;
            int v = scaling != null ? scaling.Value(action) : action.Value;
            if (action.IsAttack && dragon != null) v += dragon.NextAttackBonus;
            return v;
        }

        public IEnumerator Execute(DragonSpecies species, DragonActionData action)
        {
            if (action == null) yield break;
            string dragonName = species != null ? species.DisplayName : "드래곤";
            switch (action.Type)
            {
                case DragonActionType.Claw:
                case DragonActionType.MultiHit:
                case DragonActionType.Breath:
                    yield return Attack(species, dragonName, action);
                    break;
                case DragonActionType.Defend:
                    dragon.GainBlock(action.Value);
                    if (floating != null) floating.OnDragon($"+{action.Value}", FloatingNumberUI.Kind.Block);
                    Log(string.Format(defendFormat, dragonName, action.Value));
                    break;
                case DragonActionType.Heal:
                {
                    int h = dragon.Heal(action.Value);
                    if (floating != null) floating.OnDragon($"+{h}", FloatingNumberUI.Kind.Heal);
                    Log(string.Format(healFormat, dragonName, h));
                    break;
                }
                case DragonActionType.Buff:
                    dragon.AddAttackBonus(action.Value);
                    if (floating != null) floating.OnDragon($"공격 +{action.Value}", FloatingNumberUI.Kind.Status);
                    Log(string.Format(buffFormat, dragonName, action.Value));
                    break;
            }
        }

        IEnumerator Attack(DragonSpecies species, string dragonName, DragonActionData action)
        {
            int perHit = scaling != null ? scaling.Value(action) : action.Value;
            int bonus = dragon != null ? dragon.ConsumeAttackBonus() : 0; // 강화는 첫 번째 타격에 한 번
            int hits = action.Type == DragonActionType.MultiHit ? Mathf.Max(1, action.Hits) : 1;
            bool breath = action.Type == DragonActionType.Breath;

            if (action.PlayAttackMotion && motions != null) motions.DragonAttack();
            if (attackLead > 0f) yield return new WaitForSeconds(attackLead);

            // 연막 구르기: 이 공격 전체를 무효
            if (player.TryConsumeNegate())
            {
                if (motions != null) motions.HeroDodge();
                if (floating != null) floating.OnPlayer("무효", FloatingNumberUI.Kind.Block);
                Log(string.Format(negatedFormat, dragonName, action.DisplayName));
                yield break;
            }

            int lost = 0, blocked = 0, reflected = 0;
            for (int i = 0; i < hits; i++)
            {
                int dmg = perHit + (i == 0 ? bonus : 0);
                var (b, l) = player.TakeAttack(dmg, action.BlockIgnoreRatio);
                lost += l;
                blocked += b;
                if (floating != null)
                {
                    if (l > 0) floating.OnPlayer($"-{l}", FloatingNumberUI.Kind.Damage);
                    else if (b > 0) floating.OnPlayer("막음", FloatingNumberUI.Kind.Block);
                }
                if (motions != null)
                {
                    if (breath && action.PlayAttackMotion) motions.HeroBreath(species);
                    else motions.HeroHit(l == 0);
                }
                // 반격 자세: 맞을 때마다 반사
                if (player.Reflect > 0 && dragon != null && !dragon.IsDead)
                {
                    var (_, rl) = dragon.TakeAttack(player.Reflect);
                    reflected += rl;
                    if (floating != null && rl > 0) floating.OnDragon($"-{rl}", FloatingNumberUI.Kind.Damage);
                    if (motions != null && rl > 0) motions.DragonHurt();
                }
                if (player.IsDead || (dragon != null && dragon.IsDead)) break;
                if (i < hits - 1 && hitInterval > 0f) yield return new WaitForSeconds(hitInterval);
            }

            string text = string.Format(attackFormat, dragonName, action.DisplayName, lost);
            if (blocked > 0) text += string.Format(blockedSuffix, blocked);

            // 숨결 상태
            if (breath && action.Status != StatusType.None && action.StatusAmount > 0 && playerStatus != null && !player.IsDead)
            {
                playerStatus.Add(action.Status, action.StatusAmount);
                string name = StatusNames.Korean(action.Status);
                if (floating != null) floating.OnPlayer($"{name} +{action.StatusAmount}", FloatingNumberUI.Kind.Status);
                text += string.Format(statusSuffix, name, action.StatusAmount);
            }
            if (reflected > 0) text += string.Format(reflectSuffix, reflected);
            Log(text);
        }

        void Log(string text)
        {
            if (log != null) log.Show(text);
        }
    }
}
