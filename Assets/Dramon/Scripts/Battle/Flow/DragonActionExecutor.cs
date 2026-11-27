using System.Collections;
using UnityEngine;

namespace Dramon
{
    /// <summary>드래곤 행동 실행: 할퀴기·연타·숨결(피해)·방어 (숨결 상태·회복·강화는 2단계에서)</summary>
    public class DragonActionExecutor : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] PlayerBattleStats player;
        [SerializeField] DragonBattleStats dragon;
        [Tooltip("진행 문구")]
        [SerializeField] BattleLogMessage log;

        [Header("연출 시간")]
        [Tooltip("연타에서 한 번 때리고 다음까지 기다리는 시간(초)")]
        [Min(0f)]
        [SerializeField] float hitInterval = 0.35f;

        [Header("문구")]
        [Tooltip("{0} = 드래곤 이름, {1} = 행동 이름, {2} = 체력 피해, {3} = 방어로 막은 양")]
        [SerializeField] string attackFormat = "{0}의 {1}! {2} 피해";
        [Tooltip("방어로 막은 양이 있을 때 덧붙는 문구. {0} = 막은 양")]
        [SerializeField] string blockedSuffix = " ({0} 막음)";
        [Tooltip("{0} = 드래곤 이름, {1} = 방어")]
        [SerializeField] string defendFormat = "{0}이(가) 몸을 웅크렸다! 방어 {1}";

        /// <summary>그 행동의 한 번 피해 (1단계: 기본값 그대로)</summary>
        public int DamageOf(DragonActionData action) => action != null ? action.Value : 0;

        public IEnumerator Execute(DragonSpecies species, DragonActionData action)
        {
            if (action == null) yield break;
            string dragonName = species != null ? species.DisplayName : "드래곤";
            switch (action.Type)
            {
                case DragonActionType.Claw:
                case DragonActionType.MultiHit:
                case DragonActionType.Breath:
                {
                    int hits = action.Type == DragonActionType.MultiHit ? Mathf.Max(1, action.Hits) : 1;
                    int lost = 0, blocked = 0;
                    for (int i = 0; i < hits; i++)
                    {
                        var (b, l) = player.TakeAttack(DamageOf(action), action.BlockIgnoreRatio);
                        lost += l;
                        blocked += b;
                        if (player.IsDead) break;
                        if (i < hits - 1 && hitInterval > 0f) yield return new WaitForSeconds(hitInterval);
                    }
                    if (log != null)
                    {
                        string text = string.Format(attackFormat, dragonName, action.DisplayName, lost, blocked);
                        if (blocked > 0) text += string.Format(blockedSuffix, blocked);
                        log.Show(text);
                    }
                    break;
                }
                case DragonActionType.Defend:
                    dragon.GainBlock(action.Value);
                    if (log != null) log.Show(string.Format(defendFormat, dragonName, action.Value));
                    break;
                default:
                    // 회복·강화는 2단계에서
                    if (log != null) log.Show($"{dragonName}의 {action.DisplayName}! (2단계에서 추가)");
                    break;
            }
        }
    }
}
